using FluentValidation;
using TaskSuggestionApi.Models;

namespace TaskSuggestionApi.Validators;

public class TaskSuggestionRequestValidator : AbstractValidator<TaskSuggestionRequest>
{
    public TaskSuggestionRequestValidator()
    {
        RuleFor(x => x.Utterance)
            .NotEmpty()
            .WithMessage("Utterance is required and cannot be empty.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required and cannot be empty.");

        RuleFor(x => x.SessionId)
            .NotEmpty()
            .WithMessage("SessionId is required and cannot be empty.");

        RuleFor(x => x.Timestamp)
            .NotEmpty()
            .WithMessage("Timestamp is required.")
            .Must(BeAValidTimestamp)
            .WithMessage("Timestamp must be a valid date and time.");
    }

    private static bool BeAValidTimestamp(DateTime timestamp)
    {
        return timestamp != default && timestamp <= DateTime.UtcNow.AddMinutes(1); // Allow slight future tolerance
    }
}
