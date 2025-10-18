using FacesmashAPI.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace FacesmashAPI.Middleware
{
    /// <summary>
    /// Global exception handling middleware for comprehensive error management.
    /// Provides consistent error responses and logging across the application.
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the GlobalExceptionHandlingMiddleware.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">Logger instance for error logging.</param>
        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware to handle exceptions.
        /// </summary>
        /// <param name="context">HTTP context for the current request.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Handles exceptions and returns appropriate HTTP responses.
        /// </summary>
        /// <param name="context">HTTP context for the current request.</param>
        /// <param name="exception">The exception that occurred.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = new ErrorResponse
            {
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path,
                Method = context.Request.Method
            };

            switch (exception)
            {
                case AuthenticationException authEx:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.ErrorCode = authEx.ErrorCode;
                    errorResponse.Message = authEx.Message;
                    errorResponse.Details = "Authentication failed. Please check your credentials.";
                    break;

                case AuthorizationException authzEx:
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    errorResponse.ErrorCode = authzEx.ErrorCode;
                    errorResponse.Message = authzEx.Message;
                    errorResponse.Details = "Insufficient permissions to access this resource.";
                    break;

                case UserValidationException validationEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.ErrorCode = validationEx.ErrorCode;
                    errorResponse.Message = validationEx.Message;
                    errorResponse.Details = "User data validation failed. Please check your input.";
                    break;

                case UserNotFoundException userEx:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.ErrorCode = userEx.ErrorCode;
                    errorResponse.Message = userEx.Message;
                    errorResponse.Details = $"User with ID {userEx.UserId} was not found.";
                    break;

                case DuplicateUserException duplicateEx:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    errorResponse.ErrorCode = duplicateEx.ErrorCode;
                    errorResponse.Message = duplicateEx.Message;
                    errorResponse.Details = $"A user with {duplicateEx.Field} '{duplicateEx.Value}' already exists.";
                    break;

                case FileUploadException fileEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.ErrorCode = fileEx.ErrorCode;
                    errorResponse.Message = fileEx.Message;
                    errorResponse.Details = $"File upload failed for '{fileEx.FileName}'.";
                    break;

                case DatabaseException dbEx:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.ErrorCode = dbEx.ErrorCode;
                    errorResponse.Message = "A database error occurred.";
                    errorResponse.Details = $"Database operation '{dbEx.Operation}' failed.";
                    break;

                case ExternalApiException apiEx:
                    response.StatusCode = (int)HttpStatusCode.BadGateway;
                    errorResponse.ErrorCode = apiEx.ErrorCode;
                    errorResponse.Message = "External service error occurred.";
                    errorResponse.Details = $"External API '{apiEx.ApiName}' is currently unavailable.";
                    break;

                case BusinessLogicException businessEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.ErrorCode = businessEx.ErrorCode;
                    errorResponse.Message = businessEx.Message;
                    errorResponse.Details = $"Business rule '{businessEx.Rule}' was violated.";
                    break;

                case ArgumentNullException nullEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.ErrorCode = "NULL_ARGUMENT";
                    errorResponse.Message = "Required argument is null.";
                    errorResponse.Details = nullEx.Message;
                    break;

                case ArgumentException argEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.ErrorCode = "INVALID_ARGUMENT";
                    errorResponse.Message = "Invalid argument provided.";
                    errorResponse.Details = argEx.Message;
                    break;

                case UnauthorizedAccessException unauthorizedEx:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.ErrorCode = "UNAUTHORIZED_ACCESS";
                    errorResponse.Message = "Unauthorized access attempt.";
                    errorResponse.Details = unauthorizedEx.Message;
                    break;

                case TimeoutException timeoutEx:
                    response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    errorResponse.ErrorCode = "REQUEST_TIMEOUT";
                    errorResponse.Message = "Request timed out.";
                    errorResponse.Details = timeoutEx.Message;
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.ErrorCode = "INTERNAL_SERVER_ERROR";
                    errorResponse.Message = "An unexpected error occurred.";
                    errorResponse.Details = "Please try again later or contact support if the problem persists.";
                    break;
            }

            // Log the error with appropriate level
            if (response.StatusCode >= 500)
            {
                _logger.LogError(exception, "Server error occurred: {ErrorCode} - {Message}", 
                    errorResponse.ErrorCode, errorResponse.Message);
            }
            else if (response.StatusCode >= 400)
            {
                _logger.LogWarning(exception, "Client error occurred: {ErrorCode} - {Message}", 
                    errorResponse.ErrorCode, errorResponse.Message);
            }

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await response.WriteAsync(jsonResponse);
        }
    }

    /// <summary>
    /// Standardized error response model for consistent API error handling.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Unique error code for programmatic error handling.
        /// </summary>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable error message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Additional details about the error.
        /// </summary>
        public string Details { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the error occurred.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Request path where the error occurred.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// HTTP method of the request that caused the error.
        /// </summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// Optional trace ID for debugging purposes.
        /// </summary>
        public string? TraceId { get; set; }
    }

    /// <summary>
    /// Extension methods for registering the global exception handling middleware.
    /// </summary>
    public static class GlobalExceptionHandlingMiddlewareExtensions
    {
        /// <summary>
        /// Adds the global exception handling middleware to the application pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder for chaining.</returns>
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        }
    }
}
