using GymRatService.Common.DTOs.GeminiDTOs;  
using GymRatService.Common.DTOs.Workout;
using GymRatService.BLL.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

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

            var requestObj = new
            {
                systemInstruction = new
                {
                    parts = new[] { new { text = "You are GymRat Coach, an AI specifically designed to help people build muscle and get fit. Be friendly and motivating. You also have tools to read and manage the user's workouts and splits. Use the tools when the user asks about their workouts, schedule, or wants to delete a workout." } }
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
                                description = "Delete a specific workout from the user's library. You MUST pass the exact Guid of the workout.",
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
                                description = "Create a new blank workout routine for the user.",
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
                contents = new List<object>
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = userRequest.Message } }
                    }
                }
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

            if (firstPart?.functionCall != null)
            {
                var funcName = firstPart.functionCall.name;
                object serviceResult = null;

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
                            var wName = nameObj.ToString();
                            var wDto = new CreateWorkoutDTO { Name = wName };
                            var workout = await _workoutsService.CreateWorkoutAsync(userId, wDto);
                            serviceResult = new { success = true, workout };
                        }
                        else serviceResult = new { error = "Missing name." };
                    }
                    else if (funcName == "delete_workout")
                    {
                        if (firstPart.functionCall.args.TryGetValue("workoutId", out var wIdObj))
                        {
                            var wIdStr = wIdObj.ToString();
                            if (Guid.TryParse(wIdStr, out var wId))
                            {
                                var success = await _workoutsService.DeleteWorkoutAsync(wId, userId);
                                serviceResult = new { success, message = success ? "Workout deleted." : "Workout not found." };
                            }
                            else serviceResult = new { error = "Invalid Guid format." };
                        }
                        else serviceResult = new { error = "Missing workoutId." };
                    }
                    else serviceResult = new { error = "Unknown function call." };
                }
                catch (Exception ex)
                {
                    serviceResult = new { error = ex.Message };
                }

                requestObj.contents.Add(new
                {
                    role = "model",
                    parts = new[] { new { functionCall = firstPart.functionCall } }
                });

                requestObj.contents.Add(new
                {
                    role = "tool",
                    parts = new[] {
                        new {
                            functionResponse = new {
                                name = funcName,
                                response = new {
                                    name = funcName,
                                    content = serviceResult
                                }
                            }
                        }
                    }
                });

                var response2 = await SendWithRetryAsync(googleApiUrl, requestObj, options);
                
                if (!response2.IsSuccessStatusCode)
                {
                    var error = await response2.Content.ReadAsStringAsync();
                    return StatusCode((int)response2.StatusCode, $"Error from Gemini Function response: {error}");
                }

                var responseStr2 = await response2.Content.ReadAsStringAsync();
                var responseData2 = JsonSerializer.Deserialize<GeminiResponse>(responseStr2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var finalBotReply = responseData2?.candidates?[0]?.content?.parts?[0]?.text ?? "Nu am putut procesa mesajul.";
                return Ok(new { reply = finalBotReply });
            }

            var botReply = firstPart?.text ?? "Nu am putut procesa mesajul.";
            return Ok(new { reply = botReply });
        }
    }
}
