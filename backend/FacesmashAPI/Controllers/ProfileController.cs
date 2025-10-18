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