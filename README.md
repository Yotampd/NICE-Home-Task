# Task Suggestion API


## 1. prerequisites

You need to have .NET 8.0 SDK installed on your system.

 


## 2. Run The Server

To start the API server, navigate to the project directory and run:

```bash
cd src/TaskSuggestionApi
dotnet run
```

The API will start and be available at `http://localhost:5000`. You should see a message indicating the server is listening on this port.

## 3. Available Logic

The basic API uses keyword matching to suggest tasks based on user input. Here are the supported keywords and their corresponding tasks:

- "reset password" → ResetPasswordTask
- "forgot password" → ResetPasswordTask  
- "check order" → CheckOrderStatusTask
- "track order" → CheckOrderStatusTask

Additionally, I implemented extended keyword matching:
- "I can't remember my password" → ResetPasswordTask
- "Where is my order" → CheckOrderStatusTask
- Anything else → NoTaskFound

The matching is case-insensitive.

**Additional Features Implemented:**
- Logging for all requests and responses(printed)
- Retry mechanism

## 4. Run Tests

To verify everything is working correctly, run the following commend from the main root directory:

```bash 
dotnet test
```

This will execute all unit tests and integration tests to ensure the API functions properly.
