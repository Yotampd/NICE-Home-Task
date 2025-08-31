using Microsoft.Extensions.Logging;

namespace TaskSuggestionApi.Services;

public class TaskSuggestionService : ITaskSuggestionService
{
    private readonly ILogger<TaskSuggestionService> _logger;
    private readonly Dictionary<string, string> _taskDictionary;

    public TaskSuggestionService(ILogger<TaskSuggestionService> logger) //any class using this must have utterance
    {
        _logger = logger;
        _taskDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "reset password", "ResetPasswordTask" },
            { "forgot password", "ResetPasswordTask" },
            { "check order", "CheckOrderStatusTask" },
            { "track order", "CheckOrderStatusTask" }
        };
    }

    public async Task<string> SuggestTaskAsync(string utterance) //async is to avoid blocking the main thread
    {
        _logger.LogInformation("Processing utterance: {Utterance}", utterance);

        if (string.IsNullOrWhiteSpace(utterance)) // valid utterance
        {
            _logger.LogWarning("Empty utterance provided");
            return "NoTaskFound";
        }

        // Add retry simulated external dependency
        var task = await ExecuteWithRetryAsync(() => MatchTaskAsync(utterance)); // run the MatchTaskAsync function but with retry using ExecuteWithRetryAsync
        
        _logger.LogInformation("Suggested task: {Task} for utterance: {Utterance}", task, utterance);
        return task;
    }

    private Task<string> MatchTaskAsync(string utterance)
    {
        // Simulate potential external dependency failure
        if (ShouldSimulateFailure())
        {
            _logger.LogWarning("Simulated external dependency failure");
            throw new InvalidOperationException("External dependency failure");
        }

        var normalizedUtterance = utterance.ToLowerInvariant(); // lower case for comparison
        
        // Check for direct keyword matches
        foreach (var kvp in _taskDictionary) // check the defined dictionary, kvp is key value pair
        {
            if (normalizedUtterance.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Found matching keyword: {Keyword} -> {Task}", kvp.Key, kvp.Value);
                return Task.FromResult(kvp.Value);
            }
        }

        // Extended matching - check for variations
        if (IsPasswordRelated(normalizedUtterance))
        {
            _logger.LogDebug("Found password-related utterance -> ResetPasswordTask");
            return Task.FromResult("ResetPasswordTask");
        }

        if (IsOrderRelated(normalizedUtterance))
        {
            _logger.LogDebug("Found order-related utterance -> CheckOrderStatusTask");
            return Task.FromResult("CheckOrderStatusTask");
        }

        _logger.LogDebug("No matching task found for utterance: {Utterance}", utterance);
        return Task.FromResult("NoTaskFound");
    }

    private bool IsPasswordRelated(string utterance) // extended matching
    {
        var passwordKeywords = new[] { "password", "login", "sign in", "authenticate"};
        var actionKeywords = new[] { "reset", "forgot", "forgotten" };

        return passwordKeywords.Any(pk => utterance.Contains(pk)) || 
               actionKeywords.Any(ak => utterance.Contains(ak));
    }

    private bool IsOrderRelated(string utterance) // extended matching
    {
        var orderKeywords = new[] { "order", "purchase", "delivery", "shipment", "tracking" };
        var actionKeywords = new[] { "check", "track", "status" };
        
        return orderKeywords.Any(ok => utterance.Contains(ok)) || 
               actionKeywords.Any(ak => utterance.Contains(ak));
    }

    private async Task<string> ExecuteWithRetryAsync(Func<Task<string>> operation)
    {
        const int maxRetries = 3;
        int attempt = 0;

        while (attempt < maxRetries)
        {
            try
            {
                attempt++;
                _logger.LogDebug("Executing operation, attempt {Attempt}/{MaxRetries}", attempt, maxRetries);
                return await operation();
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                _logger.LogWarning(ex, "Operation failed on attempt {Attempt}/{MaxRetries}, retrying...", attempt, maxRetries);
                await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt)); // Exponential backoff
            }
        }

        _logger.LogError("Operation failed after {MaxRetries} attempts", maxRetries);
        throw new InvalidOperationException($"Operation failed after {maxRetries} attempts");
    }

    private static bool ShouldSimulateFailure()
    {
        // simulate failure 10% of the time 
        return new Random().NextDouble() < 0.1;
    }
}
