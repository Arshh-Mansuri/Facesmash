using FacesmashAPI.Data;
using FacesmashAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacesmashAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly AzureBlobStorageService _blobStorageService;
        private static readonly string[] Allowed = new[] { "image/jpeg", "image/png", "image/webp" };
        private const long MaxBytes = 5 * 1024 * 1024; // 5MB

        public UploadController(AppDbContext db, AzureBlobStorageService blobStorageService)
        {
            _db = db;
            _blobStorageService = blobStorageService;
        }

        /// <summary>
        /// Uploads a photo to Azure Blob Storage and updates user profile
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="file">The photo file to upload</param>
        /// <returns>The uploaded photo URL</returns>
        [HttpPost("photo")]
        public async Task<IActionResult> UploadPhoto([FromQuery] int userId, IFormFile file)
        {
            try
            {
                // Validation
                if (userId <= 0) 
                    return BadRequest(new { message = "Valid userId is required" });
                
                if (file == null || file.Length == 0) 
                    return BadRequest(new { message = "File is required" });
                
                if (file.Length > MaxBytes) 
                    return BadRequest(new { message = $"File too large. Maximum size is {MaxBytes / (1024 * 1024)}MB" });
                
                if (!Allowed.Contains(file.ContentType)) 
                    return BadRequest(new { message = "Unsupported file type. Only JPEG, PNG, and WebP are allowed" });

                // Check if user exists
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) 
                    return NotFound(new { message = "User not found" });

                // Upload to Azure Blob Storage
                var photoUrl = await _blobStorageService.UploadPhotoAsync(file, userId);

                // Update user's photo URL in database
                user.PhotoUrl = photoUrl;
                await _db.SaveChangesAsync();

                return Ok(new { 
                    message = "Photo uploaded successfully",
                    photoUrl = photoUrl,
                    userId = userId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Failed to upload photo", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Deletes a user's photo from Azure Blob Storage
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("photo")]
        public async Task<IActionResult> DeletePhoto([FromQuery] int userId)
        {
            try
            {
                if (userId <= 0) 
                    return BadRequest(new { message = "Valid userId is required" });

                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) 
                    return NotFound(new { message = "User not found" });

                if (string.IsNullOrEmpty(user.PhotoUrl))
                {
                    return Ok(new { message = "No photo to delete" });
                }

                // Delete from Azure Blob Storage
                var deleted = await _blobStorageService.DeletePhotoAsync(user.PhotoUrl);

                // Update user's photo URL in database
                user.PhotoUrl = null;
                await _db.SaveChangesAsync();

                return Ok(new { 
                    message = deleted ? "Photo deleted successfully" : "Photo deletion completed",
                    userId = userId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Failed to delete photo", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Gets all photos for a specific user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>List of photo URLs</returns>
        [HttpGet("photos")]
        public async Task<IActionResult> GetUserPhotos([FromQuery] int userId)
        {
            try
            {
                if (userId <= 0) 
                    return BadRequest(new { message = "Valid userId is required" });

                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) 
                    return NotFound(new { message = "User not found" });

                var photos = await _blobStorageService.GetUserPhotosAsync(userId);

                return Ok(new { 
                    userId = userId,
                    photos = photos,
                    count = photos.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Failed to get user photos", 
                    error = ex.Message 
                });
            }
        }
    }
}


