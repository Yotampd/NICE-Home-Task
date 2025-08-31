namespace TaskSuggestionApi.Services;

public interface ITaskSuggestionService
{
    Task<string> SuggestTaskAsync(string utterance);
}
