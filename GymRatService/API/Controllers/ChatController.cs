using GymRatService.Common.DTOs.GeminiDTOs;  
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace GymRatBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        // "Tragem" HttpClient-ul și Configurarea/Setările în controller
        public ChatController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GeminiSettings:ApiKey"] ?? "";
        }

        [HttpPost]
        public async Task<IActionResult> PostMessage([FromBody] ChatRequestDto userRequest)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                return StatusCode(500, "API Key is missing on the server.");

            var googleApiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            // Construim JSON-ul identic cu așteptările Google
            var requestBody = new GeminiRequest
            {
                contents = new[] {
                    new {
                        role = "user",
                        parts = new[] { new { text = userRequest.Message } }
                    }
                }
            };

            // Trimitem mesajul
            var response = await _httpClient.PostAsJsonAsync(googleApiUrl, requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Error from Gemini: {error}");
            }

            // Citim răspunsul
            var responseData = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            var botReply = responseData?.candidates?[0]?.content?.parts?[0]?.text ?? "Nu am putut procesa mesajul.";

            // Returnăm doar textul AI-ului (simplu și curat) către aplicația React
            return Ok(new { reply = botReply });
        }
    }
}
