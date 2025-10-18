using FacesmashAPI.Models;
using Microsoft.EntityFrameworkCore;
using FacesmashAPI.Data;
using System.Collections.Concurrent;

namespace FacesmashAPI.Services
{
    /// <summary>
    /// Service demonstrating advanced data structures and algorithms.
    /// Implements various data structures and algorithms for efficient data processing.
    /// </summary>
    public class DataStructuresAlgorithmsService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DataStructuresAlgorithmsService> _logger;

        // Advanced data structures for caching and performance
        private readonly ConcurrentDictionary<int, User> _userCache = new();
        private readonly ConcurrentQueue<User> _recentlyViewedUsers = new();
        private readonly ConcurrentStack<string> _operationHistory = new();
        private readonly HashSet<int> _activeUserIds = new();
        private readonly Dictionary<string, List<User>> _usersByGender = new();
        private readonly SortedDictionary<int, List<User>> _usersByRating = new();

        /// <summary>
        /// Initializes a new instance of the DataStructuresAlgorithmsService.
        /// </summary>
        /// <param name="context">Database context for data access.</param>
        /// <param name="logger">Logger instance for operation logging.</param>
        public DataStructuresAlgorithmsService(AppDbContext context, ILogger<DataStructuresAlgorithmsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Advanced Data Structures Operations

        /// <summary>
        /// Demonstrates HashSet operations for efficient user tracking.
        /// </summary>
        /// <returns>HashSet operations results.</returns>
        public async Task<HashSetOperationsResult> DemonstrateHashSetOperationsAsync()
        {
            try
            {
                _logger.LogInformation("Demonstrating HashSet operations");

                var users = await _context.Users.ToListAsync();
                var userIds = new HashSet<int>(users.Select(u => u.Id));

                // HashSet operations
                var operations = new HashSetOperationsResult
                {
                    TotalUsers = userIds.Count,
                    ContainsUser1 = userIds.Contains(1),
                    ContainsUser999 = userIds.Contains(999),
                    IsSubsetOfRange = userIds.IsSubsetOf(Enumerable.Range(1, 1000)),
                    IntersectionWithRange = userIds.Intersect(Enumerable.Range(1, 5)).ToHashSet(),
                    UnionWithRange = userIds.Union(Enumerable.Range(1000, 5)).ToHashSet(),
                    ExceptRange = userIds.Except(Enumerable.Range(1, 2)).ToHashSet()
                };

                _logger.LogInformation("HashSet operations completed successfully");
                return operations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during HashSet operations");
                throw;
            }
        }

        /// <summary>
        /// Demonstrates Queue operations for user activity tracking.
        /// </summary>
        /// <returns>Queue operations results.</returns>
        public async Task<QueueOperationsResult> DemonstrateQueueOperationsAsync()
        {
            try
            {
                _logger.LogInformation("Demonstrating Queue operations");

                var users = await _context.Users.Take(5).ToListAsync();
                var userQueue = new Queue<User>(users);

                var operations = new QueueOperationsResult
                {
                    InitialCount = userQueue.Count,
                    PeekedUser = userQueue.Peek()?.Name,
                    DequeuedUsers = new List<string>()
                };

                // Dequeue operations
                while (userQueue.Count > 0)
                {
                    var user = userQueue.Dequeue();
                    operations.DequeuedUsers.Add(user.Name);
                }

                operations.FinalCount = userQueue.Count;

                _logger.LogInformation("Queue operations completed successfully");
                return operations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Queue operations");
                throw;
            }
        }

        /// <summary>
        /// Demonstrates Stack operations for operation history tracking.
        /// </summary>
        /// <returns>Stack operations results.</returns>
        public async Task<StackOperationsResult> DemonstrateStackOperationsAsync()
        {
            try
            {
                _logger.LogInformation("Demonstrating Stack operations");

                var operations = new[] { "LOGIN", "VIEW_PROFILE", "SEND_MESSAGE", "VOTE", "LOGOUT" };
                var operationStack = new Stack<string>(operations);

                var result = new StackOperationsResult
                {
                    InitialCount = operationStack.Count,
                    PeekedOperation = operationStack.Peek(),
                    PoppedOperations = new List<string>()
                };

                // Pop operations
                while (operationStack.Count > 0)
                {
                    var operation = operationStack.Pop();
                    result.PoppedOperations.Add(operation);
                }

                result.FinalCount = operationStack.Count;

                _logger.LogInformation("Stack operations completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Stack operations");
                throw;
            }
        }

        /// <summary>
        /// Demonstrates Dictionary operations for efficient data lookup.
        /// </summary>
        /// <returns>Dictionary operations results.</returns>
        public async Task<DictionaryOperationsResult> DemonstrateDictionaryOperationsAsync()
        {
            try
            {
                _logger.LogInformation("Demonstrating Dictionary operations");

                var users = await _context.Users.ToListAsync();
                
                // Create dictionaries for different purposes
                var userById = users.ToDictionary(u => u.Id, u => u);
                var usersByGender = users.GroupBy(u => u.Gender).ToDictionary(g => g.Key, g => g.ToList());
                var ratingRanges = new Dictionary<string, List<User>>
                {
                    ["Low"] = users.Where(u => u.Rating < 1000).ToList(),
                    ["Medium"] = users.Where(u => u.Rating >= 1000 && u.Rating < 1500).ToList(),
                    ["High"] = users.Where(u => u.Rating >= 1500).ToList()
                };

                var operations = new DictionaryOperationsResult
                {
                    TotalUsers = userById.Count,
                    UsersByGender = usersByGender.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count),
                    RatingRanges = ratingRanges.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count),
                    ContainsUser1 = userById.ContainsKey(1),
                    User1Name = userById.TryGetValue(1, out var user1) ? user1.Name : "Not found"
                };

                _logger.LogInformation("Dictionary operations completed successfully");
                return operations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Dictionary operations");
                throw;
            }
        }

        #endregion

        #region Sorting Algorithms

        /// <summary>
        /// Demonstrates various sorting algorithms for user data.
        /// </summary>
        /// <returns>Sorting algorithms results.</returns>
        public async Task<SortingAlgorithmsResult> DemonstrateSortingAlgorithmsAsync()
        {
            try
            {
                _logger.LogInformation("Demonstrating sorting algorithms");

                var users = await _context.Users.ToListAsync();
                var userArray = users.ToArray();

                var result = new SortingAlgorithmsResult
                {
                    OriginalCount = userArray.Length,
                    SortingResults = new Dictionary<string, List<string>>()
                };

                // Bubble Sort by Name
                var bubbleSorted = BubbleSortByName(userArray.Select(u => u.Name).ToArray());
                result.SortingResults["BubbleSort"] = bubbleSorted.ToList();

                // Quick Sort by Rating
                var quickSorted = QuickSortByRating(userArray.Select(u => u.Rating).ToArray());
                result.SortingResults["QuickSort"] = quickSorted.Select(r => r.ToString()).ToList();

                // Merge Sort by Email
                var mergeSorted = MergeSortByEmail(userArray.Select(u => u.Email).ToArray());
                result.SortingResults["MergeSort"] = mergeSorted.ToList();

                // Heap Sort by ID
                var heapSorted = HeapSortById(userArray.Select(u => u.Id).ToArray());
                result.SortingResults["HeapSort"] = heapSorted.Select(i => i.ToString()).ToList();

                _logger.LogInformation("Sorting algorithms completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sorting algorithms demonstration");
                throw;
            }
        }

        /// <summary>
        /// Bubble Sort implementation for sorting names.
        /// </summary>
        private string[] BubbleSortByName(string[] names)
        {
            var sorted = (string[])names.Clone();
            int n = sorted.Length;

            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (string.Compare(sorted[j], sorted[j + 1], StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        (sorted[j], sorted[j + 1]) = (sorted[j + 1], sorted[j]);
                    }
                }
            }

            return sorted;
        }

        /// <summary>
        /// Quick Sort implementation for sorting ratings.
        /// </summary>
        private int[] QuickSortByRating(int[] ratings)
        {
            var sorted = (int[])ratings.Clone();
            QuickSort(sorted, 0, sorted.Length - 1);
            return sorted;
        }

        private void QuickSort(int[] arr, int low, int high)
        {
            if (low < high)
            {
                int pivotIndex = Partition(arr, low, high);
                QuickSort(arr, low, pivotIndex - 1);
                QuickSort(arr, pivotIndex + 1, high);
            }
        }

        private int Partition(int[] arr, int low, int high)
        {
            int pivot = arr[high];
            int i = low - 1;

            for (int j = low; j < high; j++)
            {
                if (arr[j] <= pivot)
                {
                    i++;
                    (arr[i], arr[j]) = (arr[j], arr[i]);
                }
            }

            (arr[i + 1], arr[high]) = (arr[high], arr[i + 1]);
            return i + 1;
        }

        /// <summary>
        /// Merge Sort implementation for sorting emails.
        /// </summary>
        private string[] MergeSortByEmail(string[] emails)
        {
            var sorted = (string[])emails.Clone();
            MergeSort(sorted, 0, sorted.Length - 1);
            return sorted;
        }

        private void MergeSort(string[] arr, int left, int right)
        {
            if (left < right)
            {
                int mid = left + (right - left) / 2;
                MergeSort(arr, left, mid);
                MergeSort(arr, mid + 1, right);
                Merge(arr, left, mid, right);
            }
        }

        private void Merge(string[] arr, int left, int mid, int right)
        {
            int n1 = mid - left + 1;
            int n2 = right - mid;

            string[] leftArr = new string[n1];
            string[] rightArr = new string[n2];

            Array.Copy(arr, left, leftArr, 0, n1);
            Array.Copy(arr, mid + 1, rightArr, 0, n2);

            int i = 0, j = 0, k = left;

            while (i < n1 && j < n2)
            {
                if (string.Compare(leftArr[i], rightArr[j], StringComparison.OrdinalIgnoreCase) <= 0)
                {
                    arr[k] = leftArr[i];
                    i++;
                }
                else
                {
                    arr[k] = rightArr[j];
                    j++;
                }
                k++;
            }

            while (i < n1)
            {
                arr[k] = leftArr[i];
                i++;
                k++;
            }

            while (j < n2)
            {
                arr[k] = rightArr[j];
                j++;
                k++;
            }
        }

        /// <summary>
        /// Heap Sort implementation for sorting IDs.
        /// </summary>
        private int[] HeapSortById(int[] ids)
        {
            var sorted = (int[])ids.Clone();
            int n = sorted.Length;

            // Build heap
            for (int i = n / 2 - 1; i >= 0; i--)
            {
                Heapify(sorted, n, i);
            }

            // Extract elements from heap one by one
            for (int i = n - 1; i > 0; i--)
            {
                (sorted[0], sorted[i]) = (sorted[i], sorted[0]);
                Heapify(sorted, i, 0);
            }

            return sorted;
        }

        private void Heapify(int[] arr, int n, int i)
        {
            int largest = i;
            int left = 2 * i + 1;
            int right = 2 * i + 2;

            if (left < n && arr[left] > arr[largest])
                largest = left;

            if (right < n && arr[right] > arr[largest])
                largest = right;

            if (largest != i)
            {
                (arr[i], arr[largest]) = (arr[largest], arr[i]);
                Heapify(arr, n, largest);
            }
        }

        #endregion

        #region Searching Algorithms

        /// <summary>
        /// Demonstrates various searching algorithms for user data.
        /// </summary>
        /// <returns>Searching algorithms results.</returns>
        public async Task<SearchingAlgorithmsResult> DemonstrateSearchingAlgorithmsAsync()
        {
            try
            {
                _logger.LogInformation("Demonstrating searching algorithms");

                var users = await _context.Users.ToListAsync();
                var sortedUsers = users.OrderBy(u => u.Id).ToList();
                var userIds = sortedUsers.Select(u => u.Id).ToArray();

                var result = new SearchingAlgorithmsResult
                {
                    TotalUsers = users.Count,
                    SearchResults = new Dictionary<string, object>()
                };

                // Linear Search
                var linearSearchResult = LinearSearch(userIds, 1);
                result.SearchResults["LinearSearch"] = new { Found = linearSearchResult != -1, Index = linearSearchResult };

                // Binary Search
                var binarySearchResult = BinarySearch(userIds, 1);
                result.SearchResults["BinarySearch"] = new { Found = binarySearchResult != -1, Index = binarySearchResult };

                // Interpolation Search
                var interpolationSearchResult = InterpolationSearch(userIds, 1);
                result.SearchResults["InterpolationSearch"] = new { Found = interpolationSearchResult != -1, Index = interpolationSearchResult };

                // Search by name using LINQ
                var nameSearchResult = users.Where(u => u.Name.Contains("Alice")).ToList();
                result.SearchResults["NameSearch"] = new { Count = nameSearchResult.Count, Names = nameSearchResult.Select(u => u.Name) };

                _logger.LogInformation("Searching algorithms completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during searching algorithms demonstration");
                throw;
            }
        }

        /// <summary>
        /// Linear Search implementation.
        /// </summary>
        private int LinearSearch(int[] arr, int target)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == target)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Binary Search implementation.
        /// </summary>
        private int BinarySearch(int[] arr, int target)
        {
            int left = 0, right = arr.Length - 1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;

                if (arr[mid] == target)
                    return mid;

                if (arr[mid] < target)
                    left = mid + 1;
                else
                    right = mid - 1;
            }

            return -1;
        }

        /// <summary>
        /// Interpolation Search implementation.
        /// </summary>
        private int InterpolationSearch(int[] arr, int target)
        {
            int left = 0, right = arr.Length - 1;

            while (left <= right && target >= arr[left] && target <= arr[right])
            {
                if (left == right)
                {
                    if (arr[left] == target)
                        return left;
                    return -1;
                }

                int pos = left + ((target - arr[left]) * (right - left)) / (arr[right] - arr[left]);

                if (arr[pos] == target)
                    return pos;

                if (arr[pos] < target)
                    left = pos + 1;
                else
                    right = pos - 1;
            }

            return -1;
        }

        #endregion

        #region Caching Operations

        /// <summary>
        /// Demonstrates caching operations using ConcurrentDictionary.
        /// </summary>
        /// <returns>Caching operations results.</returns>
        public async Task<CachingOperationsResult> DemonstrateCachingOperationsAsync()
        {
            try
            {
                _logger.LogInformation("Demonstrating caching operations");

                var users = await _context.Users.ToListAsync();
                
                // Populate cache
                foreach (var user in users)
                {
                    _userCache.TryAdd(user.Id, user);
                }

                var result = new CachingOperationsResult
                {
                    CacheSize = _userCache.Count,
                    CacheHitRate = CalculateCacheHitRate(),
                    CachedUsers = _userCache.Values.Take(5).Select(u => u.Name).ToList(),
                    CacheOperations = new Dictionary<string, object>()
                };

                // Test cache operations
                result.CacheOperations["ContainsUser1"] = _userCache.ContainsKey(1);
                result.CacheOperations["TryGetUser1"] = _userCache.TryGetValue(1, out var user1);
                result.CacheOperations["User1Name"] = user1?.Name ?? "Not found";

                _logger.LogInformation("Caching operations completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during caching operations demonstration");
                throw;
            }
        }

        private double CalculateCacheHitRate()
        {
            // Simulate cache hit rate calculation
            return 0.85; // 85% hit rate
        }

        #endregion
    }

    #region Result Classes

    /// <summary>
    /// Result of HashSet operations demonstration.
    /// </summary>
    public class HashSetOperationsResult
    {
        public int TotalUsers { get; set; }
        public bool ContainsUser1 { get; set; }
        public bool ContainsUser999 { get; set; }
        public bool IsSubsetOfRange { get; set; }
        public HashSet<int> IntersectionWithRange { get; set; } = new();
        public HashSet<int> UnionWithRange { get; set; } = new();
        public HashSet<int> ExceptRange { get; set; } = new();
    }

    /// <summary>
    /// Result of Queue operations demonstration.
    /// </summary>
    public class QueueOperationsResult
    {
        public int InitialCount { get; set; }
        public int FinalCount { get; set; }
        public string? PeekedUser { get; set; }
        public List<string> DequeuedUsers { get; set; } = new();
    }

    /// <summary>
    /// Result of Stack operations demonstration.
    /// </summary>
    public class StackOperationsResult
    {
        public int InitialCount { get; set; }
        public int FinalCount { get; set; }
        public string? PeekedOperation { get; set; }
        public List<string> PoppedOperations { get; set; } = new();
    }

    /// <summary>
    /// Result of Dictionary operations demonstration.
    /// </summary>
    public class DictionaryOperationsResult
    {
        public int TotalUsers { get; set; }
        public Dictionary<string, int> UsersByGender { get; set; } = new();
        public Dictionary<string, int> RatingRanges { get; set; } = new();
        public bool ContainsUser1 { get; set; }
        public string User1Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result of sorting algorithms demonstration.
    /// </summary>
    public class SortingAlgorithmsResult
    {
        public int OriginalCount { get; set; }
        public Dictionary<string, List<string>> SortingResults { get; set; } = new();
    }

    /// <summary>
    /// Result of searching algorithms demonstration.
    /// </summary>
    public class SearchingAlgorithmsResult
    {
        public int TotalUsers { get; set; }
        public Dictionary<string, object> SearchResults { get; set; } = new();
    }

    /// <summary>
    /// Result of caching operations demonstration.
    /// </summary>
    public class CachingOperationsResult
    {
        public int CacheSize { get; set; }
        public double CacheHitRate { get; set; }
        public List<string> CachedUsers { get; set; } = new();
        public Dictionary<string, object> CacheOperations { get; set; } = new();
    }

    #endregion
}
