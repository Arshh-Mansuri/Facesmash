namespace FacesmashAPI.Interfaces
{
    /// <summary>
    /// Generic interface for profile management operations.
    /// Demonstrates generic interfaces for high cohesion and low coupling.
    /// </summary>
    /// <typeparam name="T">Type of user entity (User, VipUser, etc.)</typeparam>
    public interface IProfileManager<T> where T : Models.BaseUser
    {
        /// <summary>
        /// Retrieves a user profile by ID.
        /// </summary>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <returns>User profile if found, null otherwise.</returns>
        Task<T?> GetProfileAsync(int userId);

        /// <summary>
        /// Updates a user's profile information.
        /// </summary>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <param name="name">Updated name.</param>
        /// <param name="bio">Updated biography.</param>
        /// <returns>True if update successful, false otherwise.</returns>
        Task<bool> UpdateProfileAsync(int userId, string name, string? bio);

        /// <summary>
        /// Updates a user's profile photo.
        /// </summary>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <param name="photoUrl">URL of the new profile photo.</param>
        /// <returns>True if update successful, false otherwise.</returns>
        Task<bool> UpdatePhotoAsync(int userId, string photoUrl);

        /// <summary>
        /// Gets a list of top-rated users.
        /// </summary>
        /// <param name="count">Number of users to retrieve.</param>
        /// <returns>List of top-rated users.</returns>
        Task<List<T>> GetTopRatedUsersAsync(int count = 10);

        /// <summary>
        /// Searches users by name or email.
        /// </summary>
        /// <param name="searchTerm">Term to search for.</param>
        /// <returns>List of matching users.</returns>
        Task<List<T>> SearchUsersAsync(string searchTerm);

        /// <summary>
        /// Validates user profile data.
        /// </summary>
        /// <param name="user">User object to validate.</param>
        /// <returns>True if user data is valid, false otherwise.</returns>
        bool ValidateProfile(T user);
    }
}
