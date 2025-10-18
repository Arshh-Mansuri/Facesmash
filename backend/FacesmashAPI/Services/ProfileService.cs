using FacesmashAPI.Data;
using FacesmashAPI.Interfaces;
using FacesmashAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FacesmashAPI.Services
{
    /// <summary>
    /// Service for handling profile management operations.
    /// Implements IProfileManager interface for high cohesion and low coupling.
    /// Demonstrates LINQ with lambda expressions and generic collections.
    /// </summary>
    /// <typeparam name="T">Type of user entity (User, VipUser, etc.)</typeparam>
    public class ProfileService<T> : IProfileManager<T> where T : BaseUser
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProfileService<T>> _logger;

        /// <summary>
        /// Initializes a new instance of the ProfileService.
        /// </summary>
        /// <param name="context">Database context for data access.</param>
        /// <param name="logger">Logger for profile operations.</param>
        public ProfileService(AppDbContext context, ILogger<ProfileService<T>> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a user profile by ID.
        /// </summary>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <returns>User profile if found, null otherwise.</returns>
        public async Task<T?> GetProfileAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Retrieving profile for user ID: {UserId}", userId);

                // Using LINQ with lambda expression to find user
                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .FirstOrDefaultAsync() as T;

                if (user == null)
                {
                    _logger.LogWarning("Profile not found for user ID: {UserId}", userId);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile for user ID: {UserId}", userId);
                return null;
            }
        }

        /// <summary>
        /// Updates a user's profile information.
        /// </summary>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <param name="name">Updated name.</param>
        /// <param name="bio">Updated biography.</param>
        /// <returns>True if update successful, false otherwise.</returns>
        public async Task<bool> UpdateProfileAsync(int userId, string name, string? bio)
        {
            try
            {
                _logger.LogInformation("Updating profile for user ID: {UserId}", userId);

                // Using LINQ with lambda expression to find and update user
                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    _logger.LogWarning("User not found for profile update: {UserId}", userId);
                    return false;
                }

                user.Name = name.Trim();
                user.Bio = bio?.Trim();

                await _context.SaveChangesAsync();
                _logger.LogInformation("Profile updated successfully for user ID: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user ID: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Updates a user's profile photo.
        /// </summary>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <param name="photoUrl">URL of the new profile photo.</param>
        /// <returns>True if update successful, false otherwise.</returns>
        public async Task<bool> UpdatePhotoAsync(int userId, string photoUrl)
        {
            try
            {
                _logger.LogInformation("Updating photo for user ID: {UserId}", userId);

                // Using LINQ with lambda expression to find and update user
                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    _logger.LogWarning("User not found for photo update: {UserId}", userId);
                    return false;
                }

                user.PhotoUrl = photoUrl;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Photo updated successfully for user ID: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating photo for user ID: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Gets a list of top-rated users.
        /// Demonstrates LINQ with lambda expressions and generic collections.
        /// </summary>
        /// <param name="count">Number of users to retrieve.</param>
        /// <returns>Generic collection of top-rated users.</returns>
        public async Task<List<T>> GetTopRatedUsersAsync(int count = 10)
        {
            try
            {
                _logger.LogInformation("Retrieving top {Count} rated users", count);

                // Using LINQ with lambda expressions for sorting and filtering
                var topUsers = await _context.Users
                    .Where(u => u.Rating > 0) // Filter out users with no rating
                    .OrderByDescending(u => u.Rating) // Sort by rating descending
                    .Take(count) // Take only the specified count
                    .ToListAsync() as List<T>;

                _logger.LogInformation("Retrieved {Count} top-rated users", topUsers?.Count ?? 0);
                return topUsers ?? new List<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top-rated users");
                return new List<T>();
            }
        }

        /// <summary>
        /// Searches users by name or email.
        /// Demonstrates LINQ with lambda expressions for complex filtering.
        /// </summary>
        /// <param name="searchTerm">Term to search for.</param>
        /// <returns>Generic collection of matching users.</returns>
        public async Task<List<T>> SearchUsersAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new List<T>();
                }

                _logger.LogInformation("Searching users with term: {SearchTerm}", searchTerm);

                var searchLower = searchTerm.ToLower();

                // Using LINQ with lambda expressions for complex search
                var searchResults = await _context.Users
                    .Where(u => 
                        u.Name.ToLower().Contains(searchLower) || 
                        u.Email.ToLower().Contains(searchLower) ||
                        (u.Bio != null && u.Bio.ToLower().Contains(searchLower)))
                    .OrderBy(u => u.Name) // Sort by name for consistent results
                    .ToListAsync() as List<T>;

                _logger.LogInformation("Found {Count} users matching search term: {SearchTerm}", 
                    searchResults?.Count ?? 0, searchTerm);

                return searchResults ?? new List<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users with term: {SearchTerm}", searchTerm);
                return new List<T>();
            }
        }

        /// <summary>
        /// Validates user profile data.
        /// </summary>
        /// <param name="user">User object to validate.</param>
        /// <returns>True if user data is valid, false otherwise.</returns>
        public bool ValidateProfile(T user)
        {
            try
            {
                if (user == null)
                    return false;

                // Use the polymorphic ValidateUserType method
                return user.ValidateUserType();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user profile");
                return false;
            }
        }

        /// <summary>
        /// Gets users by gender using LINQ with lambda expressions.
        /// Demonstrates anonymous methods with LINQ using lambda expressions.
        /// </summary>
        /// <param name="gender">Gender to filter by ('M' or 'F').</param>
        /// <returns>Generic collection of users matching the gender.</returns>
        public async Task<List<T>> GetUsersByGenderAsync(string gender)
        {
            try
            {
                _logger.LogInformation("Retrieving users by gender: {Gender}", gender);

                // Using LINQ with lambda expression for filtering
                var users = await _context.Users
                    .Where(u => u.Gender.ToUpper() == gender.ToUpper())
                    .OrderBy(u => u.Name)
                    .ToListAsync() as List<T>;

                _logger.LogInformation("Found {Count} users with gender: {Gender}", 
                    users?.Count ?? 0, gender);

                return users ?? new List<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users by gender: {Gender}", gender);
                return new List<T>();
            }
        }

        /// <summary>
        /// Gets user statistics using LINQ with lambda expressions.
        /// Demonstrates complex LINQ operations with anonymous methods.
        /// </summary>
        /// <returns>Dictionary containing user statistics.</returns>
        public async Task<Dictionary<string, object>> GetUserStatisticsAsync()
        {
            try
            {
                _logger.LogInformation("Calculating user statistics");

                var allUsers = await _context.Users.ToListAsync();

                // Using LINQ with lambda expressions for complex calculations
                var statistics = new Dictionary<string, object>
                {
                    ["TotalUsers"] = allUsers.Count,
                    ["AverageRating"] = allUsers.Any() ? allUsers.Average(u => u.Rating) : 0,
                    ["HighestRating"] = allUsers.Any() ? allUsers.Max(u => u.Rating) : 0,
                    ["LowestRating"] = allUsers.Any() ? allUsers.Min(u => u.Rating) : 0,
                    ["MaleUsers"] = allUsers.Count(u => u.Gender == "M"),
                    ["FemaleUsers"] = allUsers.Count(u => u.Gender == "F"),
                    ["UsersWithPhotos"] = allUsers.Count(u => !string.IsNullOrEmpty(u.PhotoUrl)),
                    ["UsersWithBio"] = allUsers.Count(u => !string.IsNullOrEmpty(u.Bio))
                };

                _logger.LogInformation("User statistics calculated successfully");
                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating user statistics");
                return new Dictionary<string, object>();
            }
        }
    }
}
