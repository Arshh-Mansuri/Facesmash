using FacesmashAPI.Data;
using FacesmashAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacesmashAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ProfileController(AppDbContext db)
        {
            _db = db;
        }

        // Helper method to get current user ID from session
        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        // Helper method to check if user is authenticated
        private bool IsAuthenticated()
        {
            return GetCurrentUserId() != null;
        }

        // GET api/profile/{id} - Public profile view
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPublicProfile([FromRoute] int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound("User not found");

            return Ok(new
            {
                user.Id,
                user.Name,
                user.PhotoUrl,
                user.Bio,
                user.Rating
            });
        }

        // GET api/profile/me - Get current user's profile
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            if (!IsAuthenticated())
                return Unauthorized(new { message = "Not logged in" });

            var userId = GetCurrentUserId();
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");

            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.PhotoUrl,
                user.Bio,
                user.Rating
            });
        }

        // POST api/profile/create - Create/complete profile (for new users)
        [HttpPost("create")]
        public async Task<IActionResult> CreateProfile([FromBody] CreateProfileRequest request)
        {
            if (!IsAuthenticated())
                return Unauthorized(new { message = "Not logged in" });

            var userId = GetCurrentUserId();
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");

            // Update user profile with provided information
            if (!string.IsNullOrWhiteSpace(request.Name))
                user.Name = request.Name;
            
            if (!string.IsNullOrWhiteSpace(request.Bio))
                user.Bio = request.Bio;
            
            if (!string.IsNullOrWhiteSpace(request.PhotoUrl))
                user.PhotoUrl = request.PhotoUrl;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.PhotoUrl,
                user.Bio,
                user.Rating,
                message = "Profile updated successfully"
            });
        }

        // PUT api/profile/me - Update current user's profile
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
        {
            if (!IsAuthenticated())
                return Unauthorized(new { message = "Not logged in" });

            var userId = GetCurrentUserId();
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");

            user.Name = request.Name ?? user.Name;
            user.Bio = request.Bio ?? user.Bio;
            if (!string.IsNullOrWhiteSpace(request.PhotoUrl))
                user.PhotoUrl = request.PhotoUrl;

            await _db.SaveChangesAsync();
            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.PhotoUrl,
                user.Bio,
                user.Rating,
                message = "Profile updated successfully"
            });
        }

        // POST api/profile/upload-photo - Upload profile photo
        [HttpPost("upload-photo")]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            if (!IsAuthenticated())
                return Unauthorized(new { message = "Not logged in" });

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest(new { message = "Invalid file type. Only JPG, PNG, and GIF files are allowed." });

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "File too large. Maximum size is 5MB." });

            try
            {
                var userId = GetCurrentUserId();
                var user = await _db.Users.FindAsync(userId);
                if (user == null) return NotFound("User not found");

                // Create uploads directory if it doesn't exist
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "users", userId.ToString());
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                // Generate unique filename
                var fileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Path.Combine(uploadsDir, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Update user's photo URL
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var photoUrl = $"{baseUrl}/uploads/users/{userId}/{fileName}";
                user.PhotoUrl = photoUrl;
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    message = "Photo uploaded successfully",
                    photoUrl = photoUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to upload photo", error = ex.Message });
            }
        }
    }

    public class UpdateProfileRequest
    {
        public string? Name { get; set; }
        public string? Bio { get; set; }
        public string? PhotoUrl { get; set; }
    }

    public class CreateProfileRequest
    {
        public string? Name { get; set; }
        public string? Bio { get; set; }
        public string? PhotoUrl { get; set; }
    }
}