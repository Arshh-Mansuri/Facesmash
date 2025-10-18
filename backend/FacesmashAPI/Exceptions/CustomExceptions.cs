using System;

namespace FacesmashAPI.Exceptions
{
    /// <summary>
    /// Base custom exception class for Facesmash application.
    /// Provides a foundation for all custom exceptions with consistent error handling.
    /// </summary>
    public abstract class FacesmashException : Exception
    {
        public string ErrorCode { get; }
        public DateTime Timestamp { get; }

        protected FacesmashException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
            Timestamp = DateTime.UtcNow;
        }

        protected FacesmashException(string errorCode, string message, Exception innerException) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            Timestamp = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Exception thrown when user authentication fails.
    /// </summary>
    public class AuthenticationException : FacesmashException
    {
        public AuthenticationException(string message) 
            : base("AUTH_FAILED", message) { }

        public AuthenticationException(string message, Exception innerException) 
            : base("AUTH_FAILED", message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when user authorization is insufficient.
    /// </summary>
    public class AuthorizationException : FacesmashException
    {
        public AuthorizationException(string message) 
            : base("AUTH_INSUFFICIENT", message) { }

        public AuthorizationException(string message, Exception innerException) 
            : base("AUTH_INSUFFICIENT", message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when user validation fails.
    /// </summary>
    public class UserValidationException : FacesmashException
    {
        public UserValidationException(string message) 
            : base("USER_VALIDATION_FAILED", message) { }

        public UserValidationException(string message, Exception innerException) 
            : base("USER_VALIDATION_FAILED", message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a user is not found.
    /// </summary>
    public class UserNotFoundException : FacesmashException
    {
        public int UserId { get; }

        public UserNotFoundException(int userId) 
            : base("USER_NOT_FOUND", $"User with ID {userId} was not found.")
        {
            UserId = userId;
        }

        public UserNotFoundException(int userId, Exception innerException) 
            : base("USER_NOT_FOUND", $"User with ID {userId} was not found.", innerException)
        {
            UserId = userId;
        }
    }

    /// <summary>
    /// Exception thrown when duplicate user data is detected.
    /// </summary>
    public class DuplicateUserException : FacesmashException
    {
        public string Field { get; }
        public string Value { get; }

        public DuplicateUserException(string field, string value) 
            : base("DUPLICATE_USER", $"User with {field} '{value}' already exists.")
        {
            Field = field;
            Value = value;
        }

        public DuplicateUserException(string field, string value, Exception innerException) 
            : base("DUPLICATE_USER", $"User with {field} '{value}' already exists.", innerException)
        {
            Field = field;
            Value = value;
        }
    }

    /// <summary>
    /// Exception thrown when file upload operations fail.
    /// </summary>
    public class FileUploadException : FacesmashException
    {
        public string FileName { get; }

        public FileUploadException(string fileName, string message) 
            : base("FILE_UPLOAD_FAILED", $"Failed to upload file '{fileName}': {message}")
        {
            FileName = fileName;
        }

        public FileUploadException(string fileName, string message, Exception innerException) 
            : base("FILE_UPLOAD_FAILED", $"Failed to upload file '{fileName}': {message}", innerException)
        {
            FileName = fileName;
        }
    }

    /// <summary>
    /// Exception thrown when database operations fail.
    /// </summary>
    public class DatabaseException : FacesmashException
    {
        public string Operation { get; }

        public DatabaseException(string operation, string message) 
            : base("DATABASE_ERROR", $"Database operation '{operation}' failed: {message}")
        {
            Operation = operation;
        }

        public DatabaseException(string operation, string message, Exception innerException) 
            : base("DATABASE_ERROR", $"Database operation '{operation}' failed: {message}", innerException)
        {
            Operation = operation;
        }
    }

    /// <summary>
    /// Exception thrown when external API calls fail.
    /// </summary>
    public class ExternalApiException : FacesmashException
    {
        public string ApiName { get; }
        public int? StatusCode { get; }

        public ExternalApiException(string apiName, string message) 
            : base("EXTERNAL_API_ERROR", $"External API '{apiName}' failed: {message}")
        {
            ApiName = apiName;
        }

        public ExternalApiException(string apiName, string message, int statusCode) 
            : base("EXTERNAL_API_ERROR", $"External API '{apiName}' failed (Status: {statusCode}): {message}")
        {
            ApiName = apiName;
            StatusCode = statusCode;
        }

        public ExternalApiException(string apiName, string message, Exception innerException) 
            : base("EXTERNAL_API_ERROR", $"External API '{apiName}' failed: {message}", innerException)
        {
            ApiName = apiName;
        }
    }

    /// <summary>
    /// Exception thrown when business logic validation fails.
    /// </summary>
    public class BusinessLogicException : FacesmashException
    {
        public string Rule { get; }

        public BusinessLogicException(string rule, string message) 
            : base("BUSINESS_LOGIC_ERROR", $"Business rule '{rule}' violated: {message}")
        {
            Rule = rule;
        }

        public BusinessLogicException(string rule, string message, Exception innerException) 
            : base("BUSINESS_LOGIC_ERROR", $"Business rule '{rule}' violated: {message}", innerException)
        {
            Rule = rule;
        }
    }
}
