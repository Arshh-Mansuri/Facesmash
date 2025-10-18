using Microsoft.EntityFrameworkCore;
using FacesmashAPI.Data;

namespace FacesmashAPI.Repositories
{
    /// <summary>
    /// Generic repository pattern implementation for database operations.
    /// Demonstrates generics and generic collections for high cohesion and low coupling.
    /// </summary>
    /// <typeparam name="T">Entity type that inherits from BaseUser</typeparam>
    public class GenericRepository<T> where T : Models.BaseUser
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        /// <summary>
        /// Initializes a new instance of the GenericRepository.
        /// </summary>
        /// <param name="context">Database context for data access.</param>
        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <summary>
        /// Retrieves all entities of type T.
        /// </summary>
        /// <returns>Generic collection of all entities.</returns>
        public async Task<List<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Retrieves an entity by its ID.
        /// </summary>
        /// <param name="id">Unique identifier of the entity.</param>
        /// <returns>Entity if found, null otherwise.</returns>
        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Adds a new entity to the database.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        /// <returns>Added entity.</returns>
        public async Task<T> AddAsync(T entity)
        {
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Updates an existing entity in the database.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <returns>Updated entity.</returns>
        public async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Deletes an entity from the database.
        /// </summary>
        /// <param name="id">ID of the entity to delete.</param>
        /// <returns>True if deletion successful, false otherwise.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                return false;

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Generic method to find entities using a predicate.
        /// Demonstrates LINQ with lambda expressions.
        /// </summary>
        /// <param name="predicate">Lambda expression to filter entities.</param>
        /// <returns>Generic collection of matching entities.</returns>
        public async Task<List<T>> FindAsync(Func<T, bool> predicate)
        {
            return await Task.FromResult(_dbSet.Where(predicate).ToList());
        }

        /// <summary>
        /// Generic method to count entities using a predicate.
        /// </summary>
        /// <param name="predicate">Lambda expression to filter entities.</param>
        /// <returns>Count of matching entities.</returns>
        public async Task<int> CountAsync(Func<T, bool> predicate)
        {
            return await Task.FromResult(_dbSet.Count(predicate));
        }

        /// <summary>
        /// Generic method to check if any entity exists matching a predicate.
        /// </summary>
        /// <param name="predicate">Lambda expression to filter entities.</param>
        /// <returns>True if any entity matches, false otherwise.</returns>
        public async Task<bool> ExistsAsync(Func<T, bool> predicate)
        {
            return await Task.FromResult(_dbSet.Any(predicate));
        }

        /// <summary>
        /// Generic method to get paginated results.
        /// </summary>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="predicate">Optional lambda expression to filter entities.</param>
        /// <returns>Generic collection of paginated entities.</returns>
        public async Task<List<T>> GetPagedAsync(int pageNumber, int pageSize, Func<T, bool>? predicate = null)
        {
            var query = predicate != null ? _dbSet.Where(predicate) : _dbSet.AsQueryable();
            
            return await Task.FromResult(query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList());
        }
    }
}
