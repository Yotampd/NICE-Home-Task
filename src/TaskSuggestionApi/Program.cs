using FluentValidation;
using Serilog;
using TaskSuggestionApi.Models;
using TaskSuggestionApi.Services;
using TaskSuggestionApi.Validators;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // use serilog for formatting the log messages
    .CreateLogger();

builder.Host.UseSerilog();

        // Add services to the container
        builder.Services.AddControllers();

// Register FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<TaskSuggestionRequestValidator>();
builder.Services.AddScoped<IValidator<TaskSuggestionRequest>, TaskSuggestionRequestValidator>();

// Register application services
builder.Services.AddScoped<ITaskSuggestionService, TaskSuggestionService>(); // one instance of the service for each request

var app = builder.Build(); // build with registered services

        // Configure the HTTP request pipeline
        app.UseHttpsRedirection();

app.MapControllers(); // make the controllers usable by the app - make them api endpoints(controller methods to http urls)

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }
