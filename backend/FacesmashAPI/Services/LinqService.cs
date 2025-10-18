using FacesmashAPI.Data;
using FacesmashAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FacesmashAPI.Services
{
    /// <summary>
    /// Service for demonstrating LINQ operations with lambda expressions.
    /// Shows various LINQ methods with anonymous methods using lambda expressions.
    /// </summary>
    public class LinqService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LinqService> _logger;

        /// <summary>
        /// Initializes a new instance of the LinqService.
        /// </summary>
        /// <param name="context">Database context for data access.</param>
        /// <param name="logger">Logger for LINQ operations.</param>
        public LinqService(AppDbContext context, ILogger<LinqService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Demonstrates LINQ Where with lambda expressions for filtering users.
        /// </summary>
        /// <param name="minRating">Minimum rating to filter by.</param>
        /// <param name="gender">Gender to filter by (optional).</param>
        /// <returns>List of users matching the criteria.</returns>
        public async Task<List<User>> GetFilteredUsersAsync(int minRating = 1000, string? gender = null)
        {
            try
            {
                _logger.LogInformation("Filtering users with min rating: {MinRating}, gender: {Gender}", minRating, gender);

                // Using LINQ Where with lambda expressions for filtering
                var query = _context.Users.AsQueryable();

                // Apply rating filter using lambda expression
                query = query.Where(user => user.Rating >= minRating);

                // Apply gender filter using lambda expression if specified
                if (!string.IsNullOrEmpty(gender))
                {
                    query = query.Where(user => user.Gender.ToUpper() == gender.ToUpper());
                }

                var result = await query.ToListAsync();

                _logger.LogInformation("Found {Count} users matching criteria", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering users");
                return new List<User>();
            }
        }

        /// <summary>
        /// Demonstrates LINQ OrderBy with lambda expressions for sorting.
        /// </summary>
        /// <param name="sortBy">Field to sort by (name, rating, createdAt).</param>
        /// <param name="ascending">Sort order (true for ascending, false for descending).</param>
        /// <returns>List of users sorted by the specified criteria.</returns>
        public async Task<List<User>> GetSortedUsersAsync(string sortBy = "name", bool ascending = true)
        {
            try
            {
                _logger.LogInformation("Sorting users by: {SortBy}, ascending: {Ascending}", sortBy, ascending);

                var query = _context.Users.AsQueryable();

                // Using LINQ OrderBy with lambda expressions for dynamic sorting
                query = sortBy.ToLower() switch
                {
                    "name" => ascending ? query.OrderBy(user => user.Name) : query.OrderByDescending(user => user.Name),
                    "rating" => ascending ? query.OrderBy(user => user.Rating) : query.OrderByDescending(user => user.Rating),
                    "createdat" => ascending ? query.OrderBy(user => user.CreatedAt) : query.OrderByDescending(user => user.CreatedAt),
                    "email" => ascending ? query.OrderBy(user => user.Email) : query.OrderByDescending(user => user.Email),
                    _ => query.OrderBy(user => user.Name) // Default to name sorting
                };

                var result = await query.ToListAsync();

                _logger.LogInformation("Retrieved {Count} sorted users", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sorting users");
                return new List<User>();
            }
        }

        /// <summary>
        /// Demonstrates LINQ Select with lambda expressions for projection.
        /// </summary>
        /// <returns>List of anonymous objects with selected user properties.</returns>
        public async Task<List<object>> GetUserProjectionsAsync()
        {
            try
            {
                _logger.LogInformation("Creating user projections");

                // Using LINQ Select with lambda expressions for projection
                var projections = await _context.Users
                    .Select(user => new
                    {
                        user.Id,
                        user.Name,
                        user.Email,
                        user.Rating,
                        user.Gender,
                        HasPhoto = !string.IsNullOrEmpty(user.PhotoUrl),
                        HasBio = !string.IsNullOrEmpty(user.Bio),
                        UserType = user.GetUserType(), // Polymorphic method call
                        DisplayInfo = user.GetDisplayInfo() // Polymorphic method call
                    })
                    .ToListAsync();

                _logger.LogInformation("Created {Count} user projections", projections.Count);
                return projections.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user projections");
                return new List<object>();
            }
        }

        /// <summary>
        /// Demonstrates LINQ GroupBy with lambda expressions for grouping.
        /// </summary>
        /// <returns>Dictionary with grouped user statistics.</returns>
        public async Task<Dictionary<string, object>> GetUserGroupingsAsync()
        {
            try
            {
                _logger.LogInformation("Creating user groupings");

                var users = await _context.Users.ToListAsync();

                // Using LINQ GroupBy with lambda expressions for grouping
                var genderGroups = users
                    .GroupBy(user => user.Gender)
                    .ToDictionary(
                        group => group.Key,
                        group => new
                        {
                            Count = group.Count(),
                            AverageRating = group.Average(user => user.Rating),
                            MaxRating = group.Max(user => user.Rating),
                            MinRating = group.Min(user => user.Rating)
                        }
                    );

                // Using LINQ GroupBy with lambda expressions for rating ranges
                var ratingGroups = users
                    .GroupBy(user => user.Rating switch
                    {
                        < 1000 => "Low",
                        >= 1000 and < 1500 => "Medium",
                        >= 1500 and < 2000 => "High",
                        >= 2000 => "Elite"
                    })
                    .ToDictionary(
                        group => group.Key,
                        group => group.Count()
                    );

                var result = new Dictionary<string, object>
                {
                    ["GenderGroups"] = genderGroups,
                    ["RatingGroups"] = ratingGroups,
                    ["TotalUsers"] = users.Count
                };

                _logger.LogInformation("Created user groupings successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user groupings");
                return new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Demonstrates LINQ Aggregate with lambda expressions for calculations.
        /// </summary>
        /// <returns>Dictionary with aggregated user statistics.</returns>
        public async Task<Dictionary<string, object>> GetUserAggregatesAsync()
        {
            try
            {
                _logger.LogInformation("Calculating user aggregates");

                var users = await _context.Users.ToListAsync();

                if (!users.Any())
                {
                    return new Dictionary<string, object>();
                }

                // Using LINQ Aggregate with lambda expressions for complex calculations
                var aggregates = new Dictionary<string, object>
                {
                    ["TotalUsers"] = users.Count,
                    ["AverageRating"] = users.Average(user => user.Rating),
                    ["MaxRating"] = users.Max(user => user.Rating),
                    ["MinRating"] = users.Min(user => user.Rating),
                    ["RatingSum"] = users.Sum(user => user.Rating),
                    ["UsersWithPhotos"] = users.Count(user => !string.IsNullOrEmpty(user.PhotoUrl)),
                    ["UsersWithBio"] = users.Count(user => !string.IsNullOrEmpty(user.Bio)),
                    ["MaleUsers"] = users.Count(user => user.Gender == "M"),
                    ["FemaleUsers"] = users.Count(user => user.Gender == "F")
                };

                // Using LINQ Aggregate with lambda expressions for custom calculations
                var ratingVariance = users.Aggregate(0.0, (sum, user) => 
                    sum + Math.Pow(user.Rating - aggregates["AverageRating"], 2)) / users.Count;

                aggregates["RatingVariance"] = ratingVariance;
                aggregates["RatingStandardDeviation"] = Math.Sqrt(ratingVariance);

                _logger.LogInformation("Calculated user aggregates successfully");
                return aggregates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating user aggregates");
                return new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Demonstrates LINQ TakeWhile and SkipWhile with lambda expressions.
        /// </summary>
        /// <param name="threshold">Rating threshold for filtering.</param>
        /// <returns>List of users above the threshold.</returns>
        public async Task<List<User>> GetUsersAboveThresholdAsync(int threshold = 1200)
        {
            try
            {
                _logger.LogInformation("Getting users above threshold: {Threshold}", threshold);

                var users = await _context.Users
                    .OrderByDescending(user => user.Rating)
                    .ToListAsync();

                // Using LINQ TakeWhile with lambda expressions
                var highRatedUsers = users
                    .TakeWhile(user => user.Rating >= threshold)
                    .ToList();

                _logger.LogInformation("Found {Count} users above threshold", highRatedUsers.Count);
                return highRatedUsers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users above threshold");
                return new List<User>();
            }
        }

        /// <summary>
        /// Demonstrates LINQ Any and All with lambda expressions for validation.
        /// </summary>
        /// <returns>Dictionary with validation results.</returns>
        public async Task<Dictionary<string, bool>> ValidateUserDataAsync()
        {
            try
            {
                _logger.LogInformation("Validating user data");

                var users = await _context.Users.ToListAsync();

                // Using LINQ Any and All with lambda expressions for validation
                var validations = new Dictionary<string, bool>
                {
                    ["HasUsers"] = users.Any(),
                    ["AllUsersHaveNames"] = users.All(user => !string.IsNullOrEmpty(user.Name)),
                    ["AllUsersHaveEmails"] = users.All(user => !string.IsNullOrEmpty(user.Email)),
                    ["AllUsersHaveValidGender"] = users.All(user => user.Gender == "M" || user.Gender == "F"),
                    ["AllUsersHaveValidRating"] = users.All(user => user.Rating >= 0 && user.Rating <= 3000),
                    ["HasHighRatedUsers"] = users.Any(user => user.Rating >= 1500),
                    ["HasUsersWithPhotos"] = users.Any(user => !string.IsNullOrEmpty(user.PhotoUrl)),
                    ["HasUsersWithBio"] = users.Any(user => !string.IsNullOrEmpty(user.Bio))
                };

                _logger.LogInformation("User data validation completed");
                return validations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user data");
                return new Dictionary<string, bool>();
            }
        }

        /// <summary>
        /// Demonstrates LINQ Join with lambda expressions for complex queries.
        /// </summary>
        /// <returns>List of joined data.</returns>
        public async Task<List<object>> GetJoinedUserDataAsync()
        {
            try
            {
                _logger.LogInformation("Creating joined user data");

                var users = await _context.Users.ToListAsync();

                // Create sample data for joining (in a real scenario, this would be from another table)
                var userCategories = new[]
                {
                    new { UserId = 1, Category = "Newcomer" },
                    new { UserId = 2, Category = "Regular" },
                    new { UserId = 3, Category = "Veteran" }
                };

                // Using LINQ Join with lambda expressions
                var joinedData = users
                    .Join(
                        userCategories,
                        user => user.Id,
                        category => category.UserId,
                        (user, category) => new
                        {
                            user.Id,
                            user.Name,
                            user.Email,
                            user.Rating,
                            category.Category,
                            UserType = user.GetUserType(), // Polymorphic method call
                            DisplayInfo = user.GetDisplayInfo() // Polymorphic method call
                        }
                    )
                    .ToList();

                _logger.LogInformation("Created {Count} joined user records", joinedData.Count);
                return joinedData.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating joined user data");
                return new List<object>();
            }
        }
    }
}
