namespace GymRatService.Common.DTOs.GeminiDTOs
{
    public class ChatRequestDto
    {
        public string Message { get; set; }
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
        public Part[] parts { get; set; }
    }
    public class Part
    {
        public string text { get; set; }
    }
}
