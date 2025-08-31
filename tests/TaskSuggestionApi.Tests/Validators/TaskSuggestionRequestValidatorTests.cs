using FluentAssertions;
using TaskSuggestionApi.Models;
using TaskSuggestionApi.Validators;
using Xunit;

namespace TaskSuggestionApi.Tests.Validators;

public class TaskSuggestionRequestValidatorTests
{
    private readonly TaskSuggestionRequestValidator _validator;

    public TaskSuggestionRequestValidatorTests()
    {
        _validator = new TaskSuggestionRequestValidator();
    }

    [Fact] // create request and run validation
    public async Task ValidateAsync_WithValidRequest_ShouldPass()
    {
        // Arrange
        var request = new TaskSuggestionRequest // basic test that proper request passes
        {
            Utterance = "I need help resetting my password",
            UserId = "12345",
            SessionId = "abcde-67890",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory] // check invalid fields - in each one of the cases replace one of the valid fields with an invalid one
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ValidateAsync_WithInvalidUtterance_ShouldFail(string utterance)
    {
        // Arrange
        var request = new TaskSuggestionRequest
        {
            Utterance = utterance,
            UserId = "12345",
            SessionId = "abcde-67890",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Utterance is required"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ValidateAsync_WithInvalidUserId_ShouldFail(string userId)
    {
        // Arrange
        var request = new TaskSuggestionRequest
        {
            Utterance = "reset password",
            UserId = userId,
            SessionId = "abcde-67890",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("UserId is required"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ValidateAsync_WithInvalidSessionId_ShouldFail(string sessionId)
    {
        // Arrange
        var request = new TaskSuggestionRequest
        {
            Utterance = "reset password",
            UserId = "12345",
            SessionId = sessionId,
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("SessionId is required"));
    }

    [Fact]
    public async Task ValidateAsync_WithDefaultTimestamp_ShouldFail()
    {
        // Arrange
        var request = new TaskSuggestionRequest
        {
            Utterance = "reset password",
            UserId = "12345",
            SessionId = "abcde-67890",
            Timestamp = default
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Timestamp must be a valid date"));
    }

    [Fact]
    public async Task ValidateAsync_WithFutureTimestamp_ShouldFail()
    {
        // Arrange
        var request = new TaskSuggestionRequest
        {
            Utterance = "reset password",
            UserId = "12345",
            SessionId = "abcde-67890",
            Timestamp = DateTime.UtcNow.AddHours(1)
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Timestamp must be a valid date"));
    }
}
