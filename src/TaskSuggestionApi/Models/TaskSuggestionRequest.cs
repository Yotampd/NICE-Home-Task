namespace TaskSuggestionApi.Models;

public class TaskSuggestionRequest
{
    public string Utterance { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
