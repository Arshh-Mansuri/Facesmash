using FacesmashAPI.Exceptions;
using FacesmashAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FacesmashAPI.Services
{
    /// <summary>
    /// Comprehensive input validation service for all user inputs.
    /// Provides centralized validation logic with detailed error reporting.
    /// </summary>
    public class InputValidationService
    {
        private readonly ILogger<InputValidationService> _logger;

        // Regular expressions for validation
        private static readonly Regex EmailRegex = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);
        private static readonly Regex PasswordRegex = new(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", RegexOptions.Compiled);
        private static readonly Regex NameRegex = new(@"^[a-zA-Z\s'-]{2,100}$", RegexOptions.Compiled);
        private static readonly Regex BioRegex = new(@"^[a-zA-Z0-9\s.,!?@#$%^&*()_+-=<>/\\|`~'"";:\[\]{}]{0,500}$", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the InputValidationService.
        /// </summary>
        /// <param name="logger">Logger instance for validation logging.</param>
        public InputValidationService(ILogger<InputValidationService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Validates user registration input comprehensively.
        /// </summary>
        /// <param name="user">User object to validate.</param>
        /// <param name="password">Plain text password to validate.</param>
        /// <returns>Validation result with detailed error information.</returns>
        public ValidationResult ValidateUserRegistration(User user, string password)
        {
            var result = new ValidationResult();
            
            try
            {
                _logger.LogInformation("Starting user registration validation for email: {Email}", user.Email);

                // Validate name
                ValidateName(user.Name, result);

                // Validate email
                ValidateEmail(user.Email, result);

                // Validate password
                ValidatePassword(password, result);

                // Validate gender
                ValidateGender(user.Gender, result);

                // Validate bio if provided
                if (!string.IsNullOrWhiteSpace(user.Bio))
                {
                    ValidateBio(user.Bio, result);
                }

                // Validate rating if provided
                ValidateRating(user.Rating, result);

                result.IsValid = !result.Errors.Any();
                
                if (result.IsValid)
                {
                    _logger.LogInformation("User registration validation successful for email: {Email}", user.Email);
                }
                else
                {
                    _logger.LogWarning("User registration validation failed for email: {Email}. Errors: {Errors}", 
                        user.Email, string.Join(", ", result.Errors));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration validation for email: {Email}", user.Email);
                result.Errors.Add("An unexpected error occurred during validation.");
                result.IsValid = false;
                return result;
            }
        }

        /// <summary>
        /// Validates user login input.
        /// </summary>
        /// <param name="email">Email address to validate.</param>
        /// <param name="password">Password to validate.</param>
        /// <returns>Validation result with detailed error information.</returns>
        public ValidationResult ValidateUserLogin(string email, string password)
        {
            var result = new ValidationResult();
            
            try
            {
                _logger.LogInformation("Starting user login validation for email: {Email}", email);

                // Validate email format
                ValidateEmail(email, result);

                // Validate password presence
                if (string.IsNullOrWhiteSpace(password))
                {
                    result.Errors.Add("Password is required.");
                }
                else if (password.Length < 1)
                {
                    result.Errors.Add("Password cannot be empty.");
                }

                result.IsValid = !result.Errors.Any();
                
                if (result.IsValid)
                {
                    _logger.LogInformation("User login validation successful for email: {Email}", email);
                }
                else
                {
                    _logger.LogWarning("User login validation failed for email: {Email}. Errors: {Errors}", 
                        email, string.Join(", ", result.Errors));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login validation for email: {Email}", email);
                result.Errors.Add("An unexpected error occurred during validation.");
                result.IsValid = false;
                return result;
            }
        }

        /// <summary>
        /// Validates message content for sending.
        /// </summary>
        /// <param name="content">Message content to validate.</param>
        /// <param name="fromUserId">Sender user ID.</param>
        /// <param name="toUserId">Recipient user ID.</param>
        /// <returns>Validation result with detailed error information.</returns>
        public ValidationResult ValidateMessage(string content, int fromUserId, int toUserId)
        {
            var result = new ValidationResult();
            
            try
            {
                _logger.LogInformation("Starting message validation from user {FromUserId} to user {ToUserId}", 
                    fromUserId, toUserId);

                // Validate user IDs
                if (fromUserId <= 0)
                {
                    result.Errors.Add("Invalid sender user ID.");
                }

                if (toUserId <= 0)
                {
                    result.Errors.Add("Invalid recipient user ID.");
                }

                if (fromUserId == toUserId)
                {
                    result.Errors.Add("Cannot send message to yourself.");
                }

                // Validate message content
                if (string.IsNullOrWhiteSpace(content))
                {
                    result.Errors.Add("Message content cannot be empty.");
                }
                else
                {
                    if (content.Length > 1000)
                    {
                        result.Errors.Add("Message content cannot exceed 1000 characters.");
                    }

                    if (content.Length < 1)
                    {
                        result.Errors.Add("Message content must contain at least 1 character.");
                    }

                    // Check for potentially harmful content
                    if (ContainsSuspiciousContent(content))
                    {
                        result.Errors.Add("Message contains potentially inappropriate content.");
                    }
                }

                result.IsValid = !result.Errors.Any();
                
                if (result.IsValid)
                {
                    _logger.LogInformation("Message validation successful from user {FromUserId} to user {ToUserId}", 
                        fromUserId, toUserId);
                }
                else
                {
                    _logger.LogWarning("Message validation failed from user {FromUserId} to user {ToUserId}. Errors: {Errors}", 
                        fromUserId, toUserId, string.Join(", ", result.Errors));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during message validation from user {FromUserId} to user {ToUserId}", 
                    fromUserId, toUserId);
                result.Errors.Add("An unexpected error occurred during validation.");
                result.IsValid = false;
                return result;
            }
        }

        /// <summary>
        /// Validates file upload parameters.
        /// </summary>
        /// <param name="fileName">Name of the file being uploaded.</param>
        /// <param name="fileSize">Size of the file in bytes.</param>
        /// <param name="contentType">MIME type of the file.</param>
        /// <returns>Validation result with detailed error information.</returns>
        public ValidationResult ValidateFileUpload(string fileName, long fileSize, string contentType)
        {
            var result = new ValidationResult();
            
            try
            {
                _logger.LogInformation("Starting file upload validation for file: {FileName}", fileName);

                // Validate file name
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    result.Errors.Add("File name is required.");
                }
                else
                {
                    if (fileName.Length > 255)
                    {
                        result.Errors.Add("File name cannot exceed 255 characters.");
                    }

                    // Check for dangerous file extensions
                    var extension = Path.GetExtension(fileName).ToLower();
                    var dangerousExtensions = new[] { ".exe", ".bat", ".cmd", ".com", ".pif", ".scr", ".vbs", ".js", ".jar" };
                    if (dangerousExtensions.Contains(extension))
                    {
                        result.Errors.Add($"File type '{extension}' is not allowed for security reasons.");
                    }
                }

                // Validate file size (5MB limit)
                const long maxFileSize = 5 * 1024 * 1024; // 5MB
                if (fileSize <= 0)
                {
                    result.Errors.Add("File size must be greater than 0.");
                }
                else if (fileSize > maxFileSize)
                {
                    result.Errors.Add($"File size cannot exceed {maxFileSize / (1024 * 1024)}MB.");
                }

                // Validate content type
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
                if (!allowedTypes.Contains(contentType))
                {
                    result.Errors.Add($"File type '{contentType}' is not supported. Allowed types: {string.Join(", ", allowedTypes)}.");
                }

                result.IsValid = !result.Errors.Any();
                
                if (result.IsValid)
                {
                    _logger.LogInformation("File upload validation successful for file: {FileName}", fileName);
                }
                else
                {
                    _logger.LogWarning("File upload validation failed for file: {FileName}. Errors: {Errors}", 
                        fileName, string.Join(", ", result.Errors));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during file upload validation for file: {FileName}", fileName);
                result.Errors.Add("An unexpected error occurred during validation.");
                result.IsValid = false;
                return result;
            }
        }

        #region Private Validation Methods

        /// <summary>
        /// Validates user name.
        /// </summary>
        private void ValidateName(string name, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                result.Errors.Add("Name is required.");
                return;
            }

            if (name.Length < 2)
            {
                result.Errors.Add("Name must be at least 2 characters long.");
            }

            if (name.Length > 100)
            {
                result.Errors.Add("Name cannot exceed 100 characters.");
            }

            if (!NameRegex.IsMatch(name))
            {
                result.Errors.Add("Name can only contain letters, spaces, hyphens, and apostrophes.");
            }
        }

        /// <summary>
        /// Validates email address.
        /// </summary>
        private void ValidateEmail(string email, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                result.Errors.Add("Email is required.");
                return;
            }

            if (email.Length > 255)
            {
                result.Errors.Add("Email cannot exceed 255 characters.");
            }

            if (!EmailRegex.IsMatch(email))
            {
                result.Errors.Add("Please enter a valid email address.");
            }
        }

        /// <summary>
        /// Validates password strength.
        /// </summary>
        private void ValidatePassword(string password, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                result.Errors.Add("Password is required.");
                return;
            }

            if (password.Length < 8)
            {
                result.Errors.Add("Password must be at least 8 characters long.");
            }

            if (password.Length > 128)
            {
                result.Errors.Add("Password cannot exceed 128 characters.");
            }

            if (!PasswordRegex.IsMatch(password))
            {
                result.Errors.Add("Password must contain at least one lowercase letter, one uppercase letter, one digit, and one special character.");
            }

            // Check for common weak passwords
            var commonPasswords = new[] { "password", "123456", "qwerty", "abc123", "password123" };
            if (commonPasswords.Contains(password.ToLower()))
            {
                result.Errors.Add("Password is too common. Please choose a more secure password.");
            }
        }

        /// <summary>
        /// Validates gender selection.
        /// </summary>
        private void ValidateGender(string gender, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(gender))
            {
                result.Errors.Add("Gender is required.");
                return;
            }

            if (gender != "M" && gender != "F")
            {
                result.Errors.Add("Gender must be 'M' for Male or 'F' for Female.");
            }
        }

        /// <summary>
        /// Validates user bio.
        /// </summary>
        private void ValidateBio(string bio, ValidationResult result)
        {
            if (bio.Length > 500)
            {
                result.Errors.Add("Bio cannot exceed 500 characters.");
            }

            if (!BioRegex.IsMatch(bio))
            {
                result.Errors.Add("Bio contains invalid characters.");
            }
        }

        /// <summary>
        /// Validates user rating.
        /// </summary>
        private void ValidateRating(int rating, ValidationResult result)
        {
            if (rating < 0)
            {
                result.Errors.Add("Rating cannot be negative.");
            }

            if (rating > 3000)
            {
                result.Errors.Add("Rating cannot exceed 3000.");
            }
        }

        /// <summary>
        /// Checks if content contains potentially suspicious patterns.
        /// </summary>
        private bool ContainsSuspiciousContent(string content)
        {
            var suspiciousPatterns = new[]
            {
                @"<script", @"javascript:", @"vbscript:", @"onload=", @"onerror=",
                @"eval\(", @"expression\(", @"url\(", @"data:text/html"
            };

            return suspiciousPatterns.Any(pattern => 
                Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase));
        }

        #endregion
    }

    /// <summary>
    /// Result of input validation containing success status and error details.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Indicates whether the validation was successful.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// List of validation error messages.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Gets the first error message if any errors exist.
        /// </summary>
        public string? FirstError => Errors.FirstOrDefault();

        /// <summary>
        /// Gets all error messages as a single string.
        /// </summary>
        public string AllErrors => string.Join("; ", Errors);
    }
}
