using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TaskSuggestionApi.Services;
using Xunit;

namespace TaskSuggestionApi.Tests.Services;

public class TaskSuggestionServiceTests
{
    private readonly Mock<ILogger<TaskSuggestionService>> _loggerMock;
    private readonly TaskSuggestionService _service;

    public TaskSuggestionServiceTests()
    {
        _loggerMock = new Mock<ILogger<TaskSuggestionService>>();
        _service = new TaskSuggestionService(_loggerMock.Object); // imitating the service layer
    }

    [Theory] // sanity check + case insensitivity check
    [InlineData("I need help resetting my password", "ResetPasswordTask")]
    [InlineData("reset password", "ResetPasswordTask")]
    [InlineData("RESET PASSWORD", "ResetPasswordTask")]
    [InlineData("forgot password", "ResetPasswordTask")]
    [InlineData("I forgot my password", "ResetPasswordTask")]
    [InlineData("check order", "CheckOrderStatusTask")]
    [InlineData("CHECK ORDER", "CheckOrderStatusTask")]
    [InlineData("track order", "CheckOrderStatusTask")]
    [InlineData("I need to track my order", "CheckOrderStatusTask")]
    public async Task SuggestTaskAsync_WithValidKeywords_ShouldReturnCorrectTask(string utterance, string expectedTask)
    {
        // Act
        var result = await _service.SuggestTaskAsync(utterance);

        // Assert
        result.Should().Be(expectedTask);
    }

    [Theory] // no matching keywords - run this multiple times
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("hello world")]
    [InlineData("random text")]
    [InlineData("help me")]
    public async Task SuggestTaskAsync_WithNoMatchingKeywords_ShouldReturnNoTaskFound(string utterance)
    {
        // Act
        var result = await _service.SuggestTaskAsync(utterance);

        // Assert
        result.Should().Be("NoTaskFound");
    }

    [Theory] // extended matching testing 
    [InlineData("I can't remember my password", "ResetPasswordTask")]
    [InlineData("I cannot remember my login credentials", "ResetPasswordTask")]
    [InlineData("I lost my password", "ResetPasswordTask")]
    [InlineData("Need to recover my password", "ResetPasswordTask")]
    [InlineData("Where is my order", "CheckOrderStatusTask")]
    [InlineData("What's the status of my purchase", "CheckOrderStatusTask")]
    [InlineData("Track my delivery", "CheckOrderStatusTask")]
    [InlineData("Check my shipment progress", "CheckOrderStatusTask")]
    public async Task SuggestTaskAsync_WithExtendedKeywords_ShouldReturnCorrectTask(string utterance, string expectedTask)
    {
        // Act
        var result = await _service.SuggestTaskAsync(utterance);

        // Assert
        result.Should().Be(expectedTask); // check if res match expected task
    }

    [Fact] 
    public async Task SuggestTaskAsync_ShouldLogInformation()
    {
        // Arrange
        var utterance = "reset password";

        // Act
        await _service.SuggestTaskAsync(utterance);

        // Assert
        _loggerMock.Verify( // logging verification - mock logger - verify that fake logger was called
            x => x.Log(
                LogLevel.Information,  // must be information
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing utterance")), // message must contain this text 
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information, // must be information
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Suggested task")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact] // test - empty utterance
    public async Task SuggestTaskAsync_WithEmptyUtterance_ShouldLogWarning()
    {
        // Act
        await _service.SuggestTaskAsync("");

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Empty utterance provided")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
