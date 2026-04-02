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

        private const string SYSTEM_PROMPT = @"You are GymRat Coach, an elite AI strength and hypertrophy specialist embedded directly into the GymRat application. 

CORE MISSION & INTENT:
Your primary objective is to build science-based, optimally structured workout routines that maximize hypertrophy and strength while preventing injury. You act as both an educational resource and an active database manager for the user's workout data. You strictly enforce biomechanical rules and do not allow the user to build sub-optimal routines.

MUSCLE GROUP & BIOMECHANICS RULES (Absolute Directives):
- PUSH: chest, front delts, lateral delts, triceps.
- PULL: lats, rhomboids, traps, rear delts, biceps, brachialis, brachioradialis.
- LEGS: quads, hamstrings, glutes, calves, hip flexors, adductors.
- Structure Minimums:
  * 'Push' MUST include: 1 chest compound + 1 shoulder isolation/compound + 1 tricep isolation.
  * 'Pull' MUST include: 1 vertical pull (lat focus) + 1 horizontal row (mid-back focus) + 1 bicep isolation.
  * 'Legs' MUST include: 1 quad-dominant compound (squat/press) + 1 hip-hinge (RDL/deadlift) + optional calf/isolation.
- Fatigue Management: Never build a Pull workout without bicep work. Never build a Push without tricep work. Never mix Push and Pull primary movers in the same session unless explicitly designing a specialized antagonist split. 

PROGRAMMING & PROGRESSIVE OVERLOAD:
- Exercise Order: Always program heavy, multi-joint compound lifts first when the central nervous system is fresh. Move to single-joint machine or cable isolation work last.
- Volume & Intensity (Sets/Reps):
  * Strength Focus: 3-5 sets, 3-6 reps (RPE 8-9).
  * Hypertrophy Focus: 3-4 sets, 6-12 reps (RIR 1-2).

TOOL EXECUTION LOGIC (Read Carefully):
You are the bridge between the user's chat and the backend database. You MUST follow these sequences:
1. ID Verification: You cannot memorize or guess Guids. If you need a Workout ID, call `get_workouts` first. If you need an Exercise ID, call `search_exercises` first.
2. Search Exercises: You HAVE a tool called `search_exercises`. Use it anytime you need to find an exercise in the database.
3. Creation Sequence: If a user says 'Create a chest day', call `create_workout`.
4. Workout Modification (IMPORTANT): You have ONE tool to modify workouts: `update_workout`. Because this tool overwrites the entire routine, if the user asks to 'add an exercise', you MUST first call `get_workouts` to see their existing exercises, then call `search_exercises` to get the ID of the new exercise, append the new exercise to the existing list, and send the FULL combined list back via `update_workout`.
5. Deletion: If a user asks to delete a workout, ALWAYS call `get_workouts` first to find the correct Guid, then call `delete_workout`.

COMMUNICATION GUARDRAILS:
- Tone: Clinical, authoritative, concise, and direct.
- Formatting: Use structured, bulleted lists.
- Autonomy: Answer general fitness questions from your knowledge without tools.";

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

            var googleApiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var contents = new List<object>();

            const int MAX_HISTORY_TURNS = 10;
            var trimmedHistory = userRequest.History
                .TakeLast(MAX_HISTORY_TURNS)
                .ToList();

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

            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = userRequest.Message } }
            });

            var requestObj = new
{
    systemInstruction = new
    {
        parts = new object[] { new { text = SYSTEM_PROMPT } }
    },
    tools = new object[]
    {
        new
        {
            functionDeclarations = new object[]
            {
                new { name = "get_workouts", description = "Get the list of all workout routines saved by the user.", parameters = new { type = "OBJECT", properties = new { } } },
                new { name = "get_weekly_schedule", description = "Get the user's personalized weekly training split schedule.", parameters = new { type = "OBJECT", properties = new { } } },
                new { name = "delete_workout", description = "Delete a specific workout from the user's library.", parameters = new { type = "OBJECT", properties = new { workoutId = new { type = "STRING", description = "The Guid ID of the workout to delete." } }, required = new string[] { "workoutId" } } },
                new { name = "create_workout", description = "Create a new blank workout routine for the user with the given name.", parameters = new { type = "OBJECT", properties = new { name = new { type = "STRING", description = "The name of the new workout." } }, required = new string[] { "name" } } },
                new { name = "search_exercises", description = "Search the global exercise database by name or muscle group.", parameters = new { type = "OBJECT", properties = new { query = new { type = "STRING", description = "The search string, e.g., 'Bench Press' or 'Chest'." } }, required = new string[] { "query" } } },
                new { name = "update_workout", description = "Updates the entire workout structure. You MUST provide the FULL list of exercises every time.", parameters = new { type = "OBJECT", properties = new { workoutId = new { type = "STRING", description = "The Guid ID of the workout." }, name = new { type = "STRING", description = "The name of the workout." }, exercises = new { type = "ARRAY", description = "The COMPLETE, updated list of exercises for this workout.", items = new { type = "OBJECT", properties = new { id = new { type = "INTEGER", description = "The integer ID of the ExerciseCard." }, targetSets = new { type = "INTEGER", description = "The number of sets for this exercise." } }, required = new string[] { "id", "targetSets" } } } }, required = new string[] { "workoutId", "name", "exercises" } }}
            } 
        }
    },
    contents = contents
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
            var parts = responseData?.candidates?[0]?.content?.parts;

            // MODIFICAREA MAJORĂ: Am schimbat "if" în "while" ca să poată apela funcții la infinit până e gata
            while (parts != null && parts.Any(p => p.functionCall != null))
            {
                var functionParts = parts.Where(p => p.functionCall != null).ToList();
                var modelTurnParts = new List<object>();
                var toolTurnParts = new List<object>();

                foreach (var part in functionParts)
                {
                    var funcName = part.functionCall.name;
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
                            if (part.functionCall.args.TryGetValue("name", out var nameObj))
                            {
                                var wDto = new CreateWorkoutDTO { Name = nameObj.ToString() };
                                var workout = await _workoutsService.CreateWorkoutAsync(userId, wDto);

                                serviceResult = new
                                {
                                    success = true,
                                    workout = new
                                    {
                                        id = workout.Id,
                                        name = workout.Name
                                    }
                                };
                            }
                            else serviceResult = new { error = "Missing name." };
                        }
                        else if (funcName == "delete_workout")
                        {
                            if (part.functionCall.args.TryGetValue("workoutId", out var wIdObj)
                                && Guid.TryParse(wIdObj.ToString(), out var wId))
                            {
                                var success = await _workoutsService.DeleteWorkoutAsync(wId, userId);
                                serviceResult = new { success, message = success ? "Workout deleted." : "Workout not found." };
                            }
                            else serviceResult = new { error = "Invalid or missing workoutId." };
                        }
                        else if (funcName == "search_exercises")
                        {
                            if (part.functionCall.args.TryGetValue("query", out var queryObj))
                            {
                                var exercises = await _workoutsService.SearchExercisesAsync(queryObj.ToString());
                                serviceResult = new { success = true, exercises };
                            }
                            else serviceResult = new { error = "Missing search query." };
                        }
                        else if (funcName == "update_workout")
                        {
                            if (part.functionCall.args.TryGetValue("workoutId", out var wIdObj) &&
                                Guid.TryParse(wIdObj.ToString(), out var wId))
                            {
                                string workoutName = part.functionCall.args.TryGetValue("name", out var n) ? n.ToString() : "Updated Workout";
                                var exercisesList = new List<UpdateWorkoutExerciseDTO>();

                                if (part.functionCall.args.TryGetValue("exercises", out var exercisesObj))
                                {
                                    var jsonElement = (JsonElement)exercisesObj;
                                    foreach (var item in jsonElement.EnumerateArray())
                                    {
                                        exercisesList.Add(new UpdateWorkoutExerciseDTO
                                        {
                                            Id = item.GetProperty("id").GetInt32(), // Am păstrat aici GetInt32() reparat
                                            TargetSets = item.GetProperty("targetSets").GetInt32()
                                        });
                                    }
                                }

                                var updateDto = new UpdateWorkoutDTO
                                {
                                    Name = workoutName,
                                    Exercises = exercisesList
                                };

                                var workout = await _workoutsService.UpdateWorkoutAsync(wId, userId, updateDto);
                                serviceResult = new { success = workout != null, message = "Workout completely updated." };
                            }
                            else serviceResult = new { error = "Invalid workoutId." };
                        }
                        else
                        {
                            serviceResult = new { error = "Unknown function." };
                        }
                    }
                    catch (Exception ex)
                    {
                        serviceResult = new { error = ex.Message };
                    }

                    modelTurnParts.Add(new { functionCall = part.functionCall });
                    toolTurnParts.Add(new
                    {
                        functionResponse = new
                        {
                            name = funcName,
                            response = new { name = funcName, content = serviceResult }
                        }
                    });
                }

                contents.Add(new { role = "model", parts = modelTurnParts });
                contents.Add(new { role = "tool", parts = toolTurnParts });

                // Trimitem din nou istoricul cu noile tool-uri rulate, ca să vedem ce zice AI-ul acum
                var response2 = await SendWithRetryAsync(googleApiUrl, requestObj, options);

                if (!response2.IsSuccessStatusCode)
                {
                    var error = await response2.Content.ReadAsStringAsync();
                    return StatusCode((int)response2.StatusCode, $"Error from Gemini function response: {error}");
                }

                var responseStr2 = await response2.Content.ReadAsStringAsync();
                responseData = JsonSerializer.Deserialize<GeminiResponse>(responseStr2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Suprascriem parts pentru a re-evalua bucla while
                parts = responseData?.candidates?[0]?.content?.parts;
            }

            // Odată ce bucla s-a oprit (AI-ul nu a mai trimis funcții), returnăm textul final!
            var botReply = parts?.FirstOrDefault(p => p.text != null)?.text ?? "Could not process the message.";
            return Ok(new { reply = botReply });
        }

    }
}