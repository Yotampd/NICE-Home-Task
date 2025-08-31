using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using TaskSuggestionApi.Models;
using TaskSuggestionApi.Services;

namespace TaskSuggestionApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskSuggestionController : ControllerBase
{
    private readonly ITaskSuggestionService _taskSuggestionService;
    private readonly IValidator<TaskSuggestionRequest> _validator;
    private readonly ILogger<TaskSuggestionController> _logger;

    public TaskSuggestionController(
        ITaskSuggestionService taskSuggestionService,
        IValidator<TaskSuggestionRequest> validator,
        ILogger<TaskSuggestionController> logger)
    {
        _taskSuggestionService = taskSuggestionService;
        _validator = validator;
        _logger = logger;
    }

    [HttpPost("suggestTask")]
    [HttpPost("~/suggestTask")] // Root level route
    public async Task<IActionResult> SuggestTask([FromBody] TaskSuggestionRequest request)
    {
        _logger.LogInformation("Received task suggestion request for userId: {UserId}, sessionId: {SessionId}", 
            request?.UserId, request?.SessionId);

        if (request == null)
        {
            _logger.LogWarning("Received null request");
            return BadRequest(new ErrorResponse
            {
                Message = "Request body is required",
                Errors = new List<string> { "Request body cannot be null" }
            });
        }

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation failed for request: {Errors}", 
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            return BadRequest(new ErrorResponse
            {
                Message = "Validation failed",
                Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
            });
        }

        try
        {
            var suggestedTask = await _taskSuggestionService.SuggestTaskAsync(request.Utterance); //using the service layer to decide the task
            
            var response = new TaskSuggestionResponse
            {
                Task = suggestedTask, //calles the serivce layer to decide the task
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Successfully processed request for userId: {UserId}, suggested task: {Task}", 
                request.UserId, suggestedTask);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing task suggestion request for userId: {UserId}", request.UserId);
            
            return StatusCode(500, new ErrorResponse
            {
                Message = "An error occurred while processing your request",
                Errors = new List<string> { "Internal server error" }
            });
        }
    }
}
