namespace GymRatService.Common.DTOs.GeminiDTOs
{
    // A single prior turn sent from the frontend (role = "user" | "ai")
    public class ChatTurnDto
    {
        public string Role { get; set; }
        public string Text { get; set; }
    }

    public class ChatRequestDto
    {
        public string Message { get; set; }
        // Last N turns from the chat UI (oldest first), excluding the current message
        public List<ChatTurnDto> History { get; set; } = new();
    }
    // Cum cerem de la Google
    public class GeminiRequest
    {
        public object[] contents { get; set; }
    }
    // Cum ne răspunde Google
    public class GeminiResponse
    {
        public Candidate[] candidates { get; set; }
    }
    public class Candidate
    {
        public Content content { get; set; }
    }
    public class Content
    {
        public string role { get; set; }
        public Part[] parts { get; set; }
    }
    public class Part
    {
        public string text { get; set; }
        public FunctionCall functionCall { get; set; }
    }
    public class FunctionCall
    {
        public string name { get; set; }
        public System.Collections.Generic.Dictionary<string, object> args { get; set; }
    }
}
