using FacesmashAPI.Data;
using FacesmashAPI.Interfaces;
using FacesmashAPI.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace FacesmashAPI.Services
{
    /// <summary>
    /// Service for handling authentication operations.
    /// Implements IAuthenticatable interface for high cohesion and low coupling.
    /// </summary>
    public class AuthenticationService : IAuthenticatable
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthenticationService> _logger;

        /// <summary>
        /// Initializes a new instance of the AuthenticationService.
        /// </summary>
        /// <param name="context">Database context for data access.</param>
        /// <param name="logger">Logger for authentication events.</param>
        public AuthenticationService(AppDbContext context, ILogger<AuthenticationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Authenticates a user with email and password.
        /// </summary>
        /// <param name="email">User's email address.</param>
        /// <param name="password">User's plain text password.</param>
        /// <returns>User object if authentication successful, null otherwise.</returns>
        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation("Attempting authentication for email: {Email}", email);

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

                if (user == null)
                {
                    _logger.LogWarning("Authentication failed: User not found for email: {Email}", email);
                    return null;
                }

                if (!VerifyPassword(password, user.PasswordHash))
                {
                    _logger.LogWarning("Authentication failed: Invalid password for email: {Email}", email);
                    return null;
                }

                _logger.LogInformation("Authentication successful for user: {UserId}", user.Id);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for email: {Email}", email);
                return null;
            }
        }

        /// <summary>
        /// Registers a new user with the provided information.
        /// </summary>
        /// <param name="name">User's display name.</param>
        /// <param name="email">User's email address.</param>
        /// <param name="password">User's plain text password.</param>
        /// <param name="gender">User's gender ('M' or 'F').</param>
        /// <returns>Created user object if successful, null otherwise.</returns>
        public async Task<User?> RegisterAsync(string name, string email, string password, string gender)
        {
            try
            {
                _logger.LogInformation("Attempting registration for email: {Email}", email);

                // Check if email is already registered
                if (await IsEmailRegisteredAsync(email))
                {
                    _logger.LogWarning("Registration failed: Email already registered: {Email}", email);
                    return null;
                }

                // Validate input
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || 
                    string.IsNullOrWhiteSpace(password) || (gender != "M" && gender != "F"))
                {
                    _logger.LogWarning("Registration failed: Invalid input data for email: {Email}", email);
                    return null;
                }

                // Create new user
                var newUser = new User
                {
                    Name = name.Trim(),
                    Email = email.ToLower().Trim(),
                    PasswordHash = HashPassword(password),
                    Gender = gender.ToUpper(),
                    Rating = 1200, // Default starting rating
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Registration successful for user: {UserId}", newUser.Id);
                return newUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", email);
                return null;
            }
        }

        /// <summary>
        /// Validates if an email is already registered.
        /// </summary>
        /// <param name="email">Email address to check.</param>
        /// <returns>True if email is already registered, false otherwise.</returns>
        public async Task<bool> IsEmailRegisteredAsync(string email)
        {
            try
            {
                return await _context.Users
                    .AnyAsync(u => u.Email.ToLower() == email.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email registration: {Email}", email);
                return false;
            }
        }

        /// <summary>
        /// Generates a secure password hash using BCrypt.
        /// </summary>
        /// <param name="password">Plain text password.</param>
        /// <returns>Hashed password string.</returns>
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Verifies a password against its hash using BCrypt.
        /// </summary>
        /// <param name="password">Plain text password.</param>
        /// <param name="hash">Hashed password to verify against.</param>
        /// <returns>True if password matches hash, false otherwise.</returns>
        public bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password hash");
                return false;
            }
        }
    }
}
