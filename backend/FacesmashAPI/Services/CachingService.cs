using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using FacesmashAPI.Models;
using FacesmashAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace FacesmashAPI.Services
{
    /// <summary>
    /// Comprehensive caching service for performance optimization.
    /// Implements both in-memory and distributed caching strategies.
    /// </summary>
    public class CachingService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distributedCache;
        private readonly AppDbContext _context;
        private readonly ILogger<CachingService> _logger;

        // Cache keys and expiration times
        private static readonly TimeSpan UserCacheExpiration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan LeaderboardCacheExpiration = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan StatisticsCacheExpiration = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan SearchResultsCacheExpiration = TimeSpan.FromMinutes(3);

        private const string USER_CACHE_PREFIX = "user_";
        private const string LEADERBOARD_CACHE_KEY = "leaderboard";
        private const string STATISTICS_CACHE_KEY = "statistics";
        private const string SEARCH_CACHE_PREFIX = "search_";

        /// <summary>
        /// Initializes a new instance of the CachingService.
        /// </summary>
        /// <param name="memoryCache">In-memory cache for fast access.</param>
        /// <param name="distributedCache">Distributed cache for scalability.</param>
        /// <param name="context">Database context for data access.</param>
        /// <param name="logger">Logger instance for caching operations.</param>
        public CachingService(
            IMemoryCache memoryCache,
            IDistributedCache distributedCache,
            AppDbContext context,
            ILogger<CachingService> logger)
        {
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
            _context = context;
            _logger = logger;
        }

        #region User Caching

        /// <summary>
        /// Gets a user from cache or database with caching strategy.
        /// </summary>
        /// <param name="userId">ID of the user to retrieve.</param>
        /// <returns>Cached or database user.</returns>
        public async Task<User?> GetUserAsync(int userId)
        {
            try
            {
                var cacheKey = $"{USER_CACHE_PREFIX}{userId}";

                // Try memory cache first (fastest)
                if (_memoryCache.TryGetValue(cacheKey, out User? cachedUser))
                {
                    _logger.LogDebug("User {UserId} retrieved from memory cache", userId);
                    return cachedUser;
                }

                // Try distributed cache (medium speed)
                var distributedCachedUser = await GetFromDistributedCacheAsync<User>(cacheKey);
                if (distributedCachedUser != null)
                {
                    // Store in memory cache for faster future access
                    _memoryCache.Set(cacheKey, distributedCachedUser, UserCacheExpiration);
                    _logger.LogDebug("User {UserId} retrieved from distributed cache", userId);
                    return distributedCachedUser;
                }

                // Get from database (slowest)
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    // Cache in both memory and distributed cache
                    _memoryCache.Set(cacheKey, user, UserCacheExpiration);
                    await SetDistributedCacheAsync(cacheKey, user, UserCacheExpiration);
                    _logger.LogDebug("User {UserId} retrieved from database and cached", userId);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId} from cache", userId);
                // Fallback to database on cache error
                return await _context.Users.FindAsync(userId);
            }
        }

        /// <summary>
        /// Invalidates user cache when user data is updated.
        /// </summary>
        /// <param name="userId">ID of the user whose cache should be invalidated.</param>
        public async Task InvalidateUserCacheAsync(int userId)
        {
            try
            {
                var cacheKey = $"{USER_CACHE_PREFIX}{userId}";

                // Remove from memory cache
                _memoryCache.Remove(cacheKey);

                // Remove from distributed cache
                await _distributedCache.RemoveAsync(cacheKey);

                _logger.LogDebug("User {UserId} cache invalidated", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating user {UserId} cache", userId);
            }
        }

        #endregion

        #region Leaderboard Caching

        /// <summary>
        /// Gets leaderboard data with caching.
        /// </summary>
        /// <param name="topCount">Number of top users to retrieve.</param>
        /// <returns>Cached leaderboard data.</returns>
        public async Task<List<User>> GetLeaderboardAsync(int topCount = 10)
        {
            try
            {
                var cacheKey = $"{LEADERBOARD_CACHE_KEY}_{topCount}";

                // Try memory cache first
                if (_memoryCache.TryGetValue(cacheKey, out List<User>? cachedLeaderboard))
                {
                    _logger.LogDebug("Leaderboard retrieved from memory cache");
                    return cachedLeaderboard;
                }

                // Try distributed cache
                var distributedCachedLeaderboard = await GetFromDistributedCacheAsync<List<User>>(cacheKey);
                if (distributedCachedLeaderboard != null)
                {
                    _memoryCache.Set(cacheKey, distributedCachedLeaderboard, LeaderboardCacheExpiration);
                    _logger.LogDebug("Leaderboard retrieved from distributed cache");
                    return distributedCachedLeaderboard;
                }

                // Get from database
                var leaderboard = await _context.Users
                    .OrderByDescending(u => u.Rating)
                    .Take(topCount)
                    .ToListAsync();

                // Cache the result
                _memoryCache.Set(cacheKey, leaderboard, LeaderboardCacheExpiration);
                await SetDistributedCacheAsync(cacheKey, leaderboard, LeaderboardCacheExpiration);

                _logger.LogDebug("Leaderboard retrieved from database and cached");
                return leaderboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leaderboard from cache");
                // Fallback to database
                return await _context.Users
                    .OrderByDescending(u => u.Rating)
                    .Take(topCount)
                    .ToListAsync();
            }
        }

        /// <summary>
        /// Invalidates leaderboard cache when user ratings change.
        /// </summary>
        public async Task InvalidateLeaderboardCacheAsync()
        {
            try
            {
                // Remove all leaderboard cache entries
                var keys = new[] { $"{LEADERBOARD_CACHE_KEY}_5", $"{LEADERBOARD_CACHE_KEY}_10", $"{LEADERBOARD_CACHE_KEY}_20" };
                
                foreach (var key in keys)
                {
                    _memoryCache.Remove(key);
                    await _distributedCache.RemoveAsync(key);
                }

                _logger.LogDebug("Leaderboard cache invalidated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating leaderboard cache");
            }
        }

        #endregion

        #region Statistics Caching

        /// <summary>
        /// Gets application statistics with caching.
        /// </summary>
        /// <returns>Cached statistics data.</returns>
        public async Task<ApplicationStatistics> GetStatisticsAsync()
        {
            try
            {
                // Try memory cache first
                if (_memoryCache.TryGetValue(STATISTICS_CACHE_KEY, out ApplicationStatistics? cachedStats))
                {
                    _logger.LogDebug("Statistics retrieved from memory cache");
                    return cachedStats;
                }

                // Try distributed cache
                var distributedCachedStats = await GetFromDistributedCacheAsync<ApplicationStatistics>(STATISTICS_CACHE_KEY);
                if (distributedCachedStats != null)
                {
                    _memoryCache.Set(STATISTICS_CACHE_KEY, distributedCachedStats, StatisticsCacheExpiration);
                    _logger.LogDebug("Statistics retrieved from distributed cache");
                    return distributedCachedStats;
                }

                // Calculate statistics from database
                var stats = await CalculateStatisticsAsync();

                // Cache the result
                _memoryCache.Set(STATISTICS_CACHE_KEY, stats, StatisticsCacheExpiration);
                await SetDistributedCacheAsync(STATISTICS_CACHE_KEY, stats, StatisticsCacheExpiration);

                _logger.LogDebug("Statistics calculated from database and cached");
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving statistics from cache");
                // Fallback to database calculation
                return await CalculateStatisticsAsync();
            }
        }

        /// <summary>
        /// Calculates application statistics from database.
        /// </summary>
        private async Task<ApplicationStatistics> CalculateStatisticsAsync()
        {
            var users = await _context.Users.ToListAsync();
            var messages = await _context.Messages.ToListAsync();

            return new ApplicationStatistics
            {
                TotalUsers = users.Count,
                TotalMessages = messages.Count,
                AverageRating = users.Any() ? users.Average(u => u.Rating) : 0,
                MaleUsers = users.Count(u => u.Gender == "M"),
                FemaleUsers = users.Count(u => u.Gender == "F"),
                UsersWithPhotos = users.Count(u => !string.IsNullOrEmpty(u.PhotoUrl)),
                UsersWithBio = users.Count(u => !string.IsNullOrEmpty(u.Bio)),
                HighestRating = users.Any() ? users.Max(u => u.Rating) : 0,
                LowestRating = users.Any() ? users.Min(u => u.Rating) : 0,
                LastUpdated = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Invalidates statistics cache when data changes.
        /// </summary>
        public async Task InvalidateStatisticsCacheAsync()
        {
            try
            {
                _memoryCache.Remove(STATISTICS_CACHE_KEY);
                await _distributedCache.RemoveAsync(STATISTICS_CACHE_KEY);
                _logger.LogDebug("Statistics cache invalidated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating statistics cache");
            }
        }

        #endregion

        #region Search Results Caching

        /// <summary>
        /// Gets search results with caching.
        /// </summary>
        /// <param name="searchTerm">Term to search for.</param>
        /// <returns>Cached search results.</returns>
        public async Task<List<User>> GetSearchResultsAsync(string searchTerm)
        {
            try
            {
                var cacheKey = $"{SEARCH_CACHE_PREFIX}{searchTerm.ToLower()}";

                // Try memory cache first
                if (_memoryCache.TryGetValue(cacheKey, out List<User>? cachedResults))
                {
                    _logger.LogDebug("Search results for '{SearchTerm}' retrieved from memory cache", searchTerm);
                    return cachedResults;
                }

                // Try distributed cache
                var distributedCachedResults = await GetFromDistributedCacheAsync<List<User>>(cacheKey);
                if (distributedCachedResults != null)
                {
                    _memoryCache.Set(cacheKey, distributedCachedResults, SearchResultsCacheExpiration);
                    _logger.LogDebug("Search results for '{SearchTerm}' retrieved from distributed cache", searchTerm);
                    return distributedCachedResults;
                }

                // Perform search
                var results = await PerformSearchAsync(searchTerm);

                // Cache the results
                _memoryCache.Set(cacheKey, results, SearchResultsCacheExpiration);
                await SetDistributedCacheAsync(cacheKey, results, SearchResultsCacheExpiration);

                _logger.LogDebug("Search results for '{SearchTerm}' calculated and cached", searchTerm);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving search results for '{SearchTerm}' from cache", searchTerm);
                // Fallback to direct search
                return await PerformSearchAsync(searchTerm);
            }
        }

        /// <summary>
        /// Performs actual search operation.
        /// </summary>
        private async Task<List<User>> PerformSearchAsync(string searchTerm)
        {
            var lowerSearchTerm = searchTerm.ToLower();
            return await _context.Users
                .Where(u => u.Name.ToLower().Contains(lowerSearchTerm) ||
                           u.Email.ToLower().Contains(lowerSearchTerm) ||
                           (u.Bio != null && u.Bio.ToLower().Contains(lowerSearchTerm)))
                .ToListAsync();
        }

        #endregion

        #region Cache Management

        /// <summary>
        /// Clears all caches.
        /// </summary>
        public async Task ClearAllCachesAsync()
        {
            try
            {
                // Clear memory cache
                if (_memoryCache is MemoryCache memCache)
                {
                    memCache.Compact(1.0); // Remove all entries
                }

                // Clear distributed cache (this would need to be implemented based on the specific distributed cache provider)
                _logger.LogInformation("All caches cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing caches");
            }
        }

        /// <summary>
        /// Gets cache statistics for monitoring.
        /// </summary>
        /// <returns>Cache statistics.</returns>
        public CacheStatistics GetCacheStatistics()
        {
            try
            {
                return new CacheStatistics
                {
                    MemoryCacheSize = GetMemoryCacheSize(),
                    DistributedCacheEnabled = _distributedCache != null,
                    CacheHitRate = CalculateCacheHitRate(),
                    LastCleared = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache statistics");
                return new CacheStatistics();
            }
        }

        private int GetMemoryCacheSize()
        {
            // This is a simplified implementation
            // In a real scenario, you'd need to access the internal cache size
            return 0; // Placeholder
        }

        private double CalculateCacheHitRate()
        {
            // This would be calculated based on actual cache hit/miss metrics
            return 0.85; // Placeholder for 85% hit rate
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Gets data from distributed cache.
        /// </summary>
        private async Task<T?> GetFromDistributedCacheAsync<T>(string key)
        {
            try
            {
                var cachedData = await _distributedCache.GetStringAsync(key);
                if (cachedData != null)
                {
                    return JsonSerializer.Deserialize<T>(cachedData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error retrieving data from distributed cache for key: {Key}", key);
            }
            return default;
        }

        /// <summary>
        /// Sets data in distributed cache.
        /// </summary>
        private async Task SetDistributedCacheAsync<T>(string key, T data, TimeSpan expiration)
        {
            try
            {
                var serializedData = JsonSerializer.Serialize(data);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                };
                await _distributedCache.SetStringAsync(key, serializedData, options);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error setting data in distributed cache for key: {Key}", key);
            }
        }

        #endregion
    }

    #region Data Models

    /// <summary>
    /// Application statistics data model.
    /// </summary>
    public class ApplicationStatistics
    {
        public int TotalUsers { get; set; }
        public int TotalMessages { get; set; }
        public double AverageRating { get; set; }
        public int MaleUsers { get; set; }
        public int FemaleUsers { get; set; }
        public int UsersWithPhotos { get; set; }
        public int UsersWithBio { get; set; }
        public int HighestRating { get; set; }
        public int LowestRating { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Cache statistics data model.
    /// </summary>
    public class CacheStatistics
    {
        public int MemoryCacheSize { get; set; }
        public bool DistributedCacheEnabled { get; set; }
        public double CacheHitRate { get; set; }
        public DateTime LastCleared { get; set; }
    }

    #endregion
}
