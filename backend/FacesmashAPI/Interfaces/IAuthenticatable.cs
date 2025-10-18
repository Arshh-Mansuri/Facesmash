namespace FacesmashAPI.Interfaces
{
    /// <summary>
    /// Interface for authentication operations.
    /// Demonstrates interface usage for high cohesion and low coupling.
    /// </summary>
    public interface IAuthenticatable
    {
        /// <summary>
        /// Authenticates a user with email and password.
        /// </summary>
        /// <param name="email">User's email address.</param>
        /// <param name="password">User's plain text password.</param>
        /// <returns>User object if authentication successful, null otherwise.</returns>
        Task<Models.User?> AuthenticateAsync(string email, string password);

        /// <summary>
        /// Registers a new user with the provided information.
        /// </summary>
        /// <param name="name">User's display name.</param>
        /// <param name="email">User's email address.</param>
        /// <param name="password">User's plain text password.</param>
        /// <param name="gender">User's gender ('M' or 'F').</param>
        /// <returns>Created user object if successful, null otherwise.</returns>
        Task<Models.User?> RegisterAsync(string name, string email, string password, string gender);

        /// <summary>
        /// Validates if an email is already registered.
        /// </summary>
        /// <param name="email">Email address to check.</param>
        /// <returns>True if email is already registered, false otherwise.</returns>
        Task<bool> IsEmailRegisteredAsync(string email);

        /// <summary>
        /// Generates a secure password hash.
        /// </summary>
        /// <param name="password">Plain text password.</param>
        /// <returns>Hashed password string.</returns>
        string HashPassword(string password);

        /// <summary>
        /// Verifies a password against its hash.
        /// </summary>
        /// <param name="password">Plain text password.</param>
        /// <param name="hash">Hashed password to verify against.</param>
        /// <returns>True if password matches hash, false otherwise.</returns>
        bool VerifyPassword(string password, string hash);
    }
}
