using GymRatService.Common.DTOs.GeminiDTOs;
using GymRatService.Common.DTOs.Workout;
using GymRatService.BLL.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace GymRatBackend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly IWorkoutsService _workoutsService;
        private readonly IPersonalizedSplitsService _personalizedSplitsService;

        // Compact, domain-authoritative system prompt.
        // Encodes muscle anatomy upfront so Gemini doesn't hallucinate muscle group membership.
        // Written in imperative style (no pleasantries) to reduce prompt token count.
        private const string SYSTEM_PROMPT = @"You are GymRat Coach, an expert AI strength & hypertrophy coach embedded in the GymRat app.

MUSCLE GROUP RULES (never violate these):
- PUSH muscles: chest, front delts, lateral delts, triceps
- PULL muscles: lats, rhomboids, traps, rear delts, biceps, brachialis, brachioradialis (forearms)
- LEGS muscles: quads, hamstrings, glutes, calves, hip flexors, adductors
- A 'Push' workout MUST include at minimum: a chest compound + shoulder work + tricep isolation
- A 'Pull' workout MUST include at minimum: a vertical pull (lat focus) + horizontal row (rear delt/rhomboid) + bicep isolation
- A 'Legs' workout MUST include at minimum: a quad compound (squat pattern) + hip-hinge (deadlift/RDL pattern) + optional isolation
- Never build a Pull workout without biceps. Never build a Push without triceps. Never mix Push + Pull primary movers.

WORKOUT STRUCTURE:
- Order: compound lifts first, isolation last
- Typical sets: 3-5 for compounds, 3-4 for isolation
- Rep ranges: strength 3-6, hypertrophy 6-12, endurance 12-20

BEHAVIOR:
- Be concise and direct. No filler phrases.
- When creating a workout plan respond in a clean, structured list.
- You have tools to read/manage the user's workouts and weekly split. Use them when the user asks about their data or wants to create/delete a workout.
- Never invent workout IDs. Always call get_workouts first if you need an ID.
- If the user asks a general fitness question, answer from your knowledge without calling tools.";

        public ChatController(HttpClient httpClient, IConfiguration configuration,
            IWorkoutsService workoutsService, IPersonalizedSplitsService personalizedSplitsService)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GeminiSettings:ApiKey"] ?? "";
            _workoutsService = workoutsService;
            _personalizedSplitsService = personalizedSplitsService;
        }

        private async Task<HttpResponseMessage> SendWithRetryAsync(string url, object body, JsonSerializerOptions options)
        {
            int[] delays = { 5000, 10000, 15000 };
            HttpResponseMessage response = null;
            for (int i = 0; i <= delays.Length; i++)
            {
                response = await _httpClient.PostAsJsonAsync(url, body, options);
                if ((int)response.StatusCode != 429) return response;
                if (i < delays.Length)
                    await Task.Delay(delays[i]);
            }
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> PostMessage([FromBody] ChatRequestDto userRequest)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                return StatusCode(500, "API Key is missing on the server.");

            var googleApiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ── Build multi-turn contents from history + current message ──────────
            // Gemini requires alternating user/model roles in contents.
            // History from the frontend uses role "user"/"ai"; we map "ai" → "model".
            var contents = new List<object>();

            const int MAX_HISTORY_TURNS = 10; // cap to limit token spend
            var trimmedHistory = userRequest.History
                .TakeLast(MAX_HISTORY_TURNS)
                .ToList();

            // Gemini requires contents to start with a 'user' turn — drop leading 'model' turns.
            while (trimmedHistory.Count > 0 && trimmedHistory[0].Role?.ToLower() == "ai")
                trimmedHistory.RemoveAt(0);

            foreach (var turn in trimmedHistory)
            {
                var geminiRole = turn.Role == "ai" ? "model" : "user";
                contents.Add(new
                {
                    role = geminiRole,
                    parts = new[] { new { text = turn.Text } }
                });
            }

            // Append the current user message
            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = userRequest.Message } }
            });

            // ── Build the full request ────────────────────────────────────────────
            var requestObj = new
            {
                systemInstruction = new
                {
                    parts = new[] { new { text = SYSTEM_PROMPT } }
                },
                tools = new[]
                {
                    new
                    {
                        functionDeclarations = new object[]
                        {
                            new {
                                name = "get_workouts",
                                description = "Get the list of all workout routines saved by the user.",
                                parameters = new { type = "OBJECT", properties = new { } }
                            },
                            new {
                                name = "get_weekly_schedule",
                                description = "Get the user's personalized weekly training split schedule.",
                                parameters = new { type = "OBJECT", properties = new { } }
                            },
                            new {
                                name = "delete_workout",
                                description = "Delete a specific workout from the user's library. Call get_workouts first to obtain the exact Guid.",
                                parameters = new {
                                    type = "OBJECT",
                                    properties = new {
                                        workoutId = new {
                                            type = "STRING",
                                            description = "The Guid ID of the workout to delete."
                                        }
                                    },
                                    required = new[] { "workoutId" }
                                }
                            },
                            new {
                                name = "create_workout",
                                description = "Create a new blank workout routine for the user with the given name.",
                                parameters = new {
                                    type = "OBJECT",
                                    properties = new {
                                        name = new {
                                            type = "STRING",
                                            description = "The name of the new workout."
                                        }
                                    },
                                    required = new[] { "name" }
                                }
                            }
                        }
                    }
                },
                contents
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var response = await SendWithRetryAsync(googleApiUrl, requestObj, options);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Error from Gemini: {error}");
            }

            var responseStr = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<GeminiResponse>(responseStr, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var firstPart = responseData?.candidates?[0]?.content?.parts?[0];

            // ── Handle tool call ──────────────────────────────────────────────────
            if (firstPart?.functionCall != null)
            {
                var funcName = firstPart.functionCall.name;
                object serviceResult;

                try
                {
                    if (funcName == "get_workouts")
                    {
                        serviceResult = await _workoutsService.GetFormattedWorkoutsByUserIdAsync(userId);
                    }
                    else if (funcName == "get_weekly_schedule")
                    {
                        serviceResult = await _personalizedSplitsService.GetUserWeeklySplitByUserIdAsync(userId);
                    }
                    else if (funcName == "create_workout")
                    {
                        if (firstPart.functionCall.args.TryGetValue("name", out var nameObj))
                        {
                            var wDto = new CreateWorkoutDTO { Name = nameObj.ToString() };
                            var workout = await _workoutsService.CreateWorkoutAsync(userId, wDto);
                            serviceResult = new { success = true, workout };
                        }
                        else serviceResult = new { error = "Missing name." };
                    }
                    else if (funcName == "delete_workout")
                    {
                        if (firstPart.functionCall.args.TryGetValue("workoutId", out var wIdObj)
                            && Guid.TryParse(wIdObj.ToString(), out var wId))
                        {
                            var success = await _workoutsService.DeleteWorkoutAsync(wId, userId);
                            serviceResult = new { success, message = success ? "Workout deleted." : "Workout not found." };
                        }
                        else serviceResult = new { error = "Invalid or missing workoutId." };
                    }
                    else serviceResult = new { error = "Unknown function." };
                }
                catch (Exception ex)
                {
                    serviceResult = new { error = ex.Message };
                }

                // Append model's function call + tool result to contents and call Gemini again
                contents.Add(new
                {
                    role = "model",
                    parts = new[] { new { functionCall = firstPart.functionCall } }
                });

                contents.Add(new
                {
                    role = "tool",
                    parts = new[] {
                        new {
                            functionResponse = new {
                                name = funcName,
                                response = new { name = funcName, content = serviceResult }
                            }
                        }
                    }
                });

                var response2 = await SendWithRetryAsync(googleApiUrl, requestObj, options);

                if (!response2.IsSuccessStatusCode)
                {
                    var error = await response2.Content.ReadAsStringAsync();
                    return StatusCode((int)response2.StatusCode, $"Error from Gemini function response: {error}");
                }

                var responseStr2 = await response2.Content.ReadAsStringAsync();
                var responseData2 = JsonSerializer.Deserialize<GeminiResponse>(responseStr2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var finalReply = responseData2?.candidates?[0]?.content?.parts?[0]?.text ?? "Could not process the message.";
                return Ok(new { reply = finalReply });
            }

            var botReply = firstPart?.text ?? "Could not process the message.";
            return Ok(new { reply = botReply });
        }
    }
}
