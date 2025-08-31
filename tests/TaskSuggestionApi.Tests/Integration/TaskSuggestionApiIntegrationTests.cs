using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TaskSuggestionApi.Models;
using Xunit;

namespace TaskSuggestionApi.Tests.Integration;

public class TaskSuggestionApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>> //webserver in memory
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public TaskSuggestionApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Theory] //valid request test cases - test 1
    [InlineData("I need help resetting my password", "ResetPasswordTask")]
    [InlineData("reset password", "ResetPasswordTask")]
    [InlineData("forgot password", "ResetPasswordTask")]
    [InlineData("check order", "CheckOrderStatusTask")]
    [InlineData("track order", "CheckOrderStatusTask")]
    public async Task SuggestTask_WithValidRequest_ShouldReturnCorrectTask(string utterance, string expectedTask)
    {
        // Arrange
        var request = new TaskSuggestionRequest
        {
            Utterance = utterance,
            UserId = "12345",
            SessionId = "abcde-67890",
            Timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/suggestTask", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // pasrse response to C# object
        var responseContent = await response.Content.ReadAsStringAsync();
        var taskResponse = JsonSerializer.Deserialize<TaskSuggestionResponse>(responseContent, _jsonOptions);
        
        taskResponse.Should().NotBeNull();
        taskResponse!.Task.Should().Be(expectedTask);
        taskResponse.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact] // test 2 - no task found
    public async Task SuggestTask_WithNoMatchingKeywords_ShouldReturnNoTaskFound()
    {
        // Arrange - no task found test
        var request = new TaskSuggestionRequest
        {
            Utterance = "hello world",
            UserId = "12345",
            SessionId = "abcde-67890",
            Timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/suggestTask", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var taskResponse = JsonSerializer.Deserialize<TaskSuggestionResponse>(responseContent, _jsonOptions);
        
        taskResponse.Should().NotBeNull();
        taskResponse!.Task.Should().Be("NoTaskFound");
    }

    [Fact] // test 3 - invalid request
    public async Task SuggestTask_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new TaskSuggestionRequest
        {
            Utterance = "", // Invalid empty utterance
            UserId = "12345",
            SessionId = "abcde-67890",
            Timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/suggestTask", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest); // check that validatation error returns http 400
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, _jsonOptions);
        
        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Be("Validation failed");
        errorResponse.Errors.Should().Contain(e => e.Contains("Utterance is required")); // verify error message
    }



    [Fact] // test 5 - broken json
    public async Task SuggestTask_WithMalformedJson_ShouldReturnBadRequest()
    {
        // Arrange
        var content = new StringContent("{ invalid json }", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/suggestTask", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory] // test 6 - extended matching
    [InlineData("I can't remember my password", "ResetPasswordTask")]
    [InlineData("I forgot my login credentials", "ResetPasswordTask")]
    [InlineData("Where is my order", "CheckOrderStatusTask")]
    [InlineData("What's the status of my purchase", "CheckOrderStatusTask")]
    public async Task SuggestTask_WithExtendedKeywords_ShouldReturnCorrectTask(string utterance, string expectedTask)
    {
        // Arrange
        var request = new TaskSuggestionRequest
        {
            Utterance = utterance,
            UserId = "12345",
            SessionId = "abcde-67890",
            Timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/suggestTask", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var taskResponse = JsonSerializer.Deserialize<TaskSuggestionResponse>(responseContent, _jsonOptions);
        
        taskResponse.Should().NotBeNull();
        taskResponse!.Task.Should().Be(expectedTask);
    }
}
