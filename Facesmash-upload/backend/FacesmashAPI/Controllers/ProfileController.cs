using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FacesmashAPI.Data;
using FacesmashAPI.Models;
using Microsoft.AspNetCore.Http;

namespace FacesmashAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProfileController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.Rating)
                .ToListAsync();
            return Ok(users);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("User not found");
            return Ok(user);
        }

        [HttpPost("create")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> CreateProfile([FromForm] string name, [FromForm] string email, [FromForm] string? bio, [FromForm] string? gender, [FromForm] IFormFile? photo)
        {
            // Check if user with this email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                return BadRequest("User with this email already exists");
            }

            var user = new User
            {
                Name = name,
                Email = email,
                Bio = bio,
                Gender = gender ?? "M",
                Rating = 1200, // Default rating
                PasswordHash = "default" // Simple default for now
            };

            if (photo != null && photo.Length > 0)
            {
                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
                var fileExtension = Path.GetExtension(photo.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Invalid file type. Only JPG, JPEG, PNG, GIF, WebP, and BMP files are allowed.");
                }

                // Validate file size (10MB limit)
                const long maxFileSize = 10 * 1024 * 1024; // 10MB
                if (photo.Length > maxFileSize)
                {
                    return BadRequest("File size must be less than 10MB.");
                }

                var uploadsRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
                if (!Directory.Exists(uploadsRoot)) Directory.CreateDirectory(uploadsRoot);
                var fileName = $"user_{Guid.NewGuid():N}{fileExtension}";
                var filePath = Path.Combine(uploadsRoot, fileName);
                
                using (var stream = System.IO.File.Create(filePath))
                {
                    await photo.CopyToAsync(stream);
                }
                user.PhotoUrl = $"/uploads/{fileName}";
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            return Ok(user);
        }

        [HttpPost("{id:int}/update")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> UpdateProfile(int id, [FromForm] string name, [FromForm] string email, [FromForm] string? bio, [FromForm] string? gender, [FromForm] IFormFile? photo)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("User not found");

            user.Name = name;
            user.Email = email;
            user.Bio = bio;
            user.Gender = gender ?? user.Gender;

            if (photo != null && photo.Length > 0)
            {
                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
                var fileExtension = Path.GetExtension(photo.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Invalid file type. Only JPG, JPEG, PNG, GIF, WebP, and BMP files are allowed.");
                }

                // Validate file size (10MB limit)
                const long maxFileSize = 10 * 1024 * 1024; // 10MB
                if (photo.Length > maxFileSize)
                {
                    return BadRequest("File size must be less than 10MB.");
                }

                var uploadsRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
                if (!Directory.Exists(uploadsRoot)) Directory.CreateDirectory(uploadsRoot);
                var fileName = $"user_{id}_{Guid.NewGuid():N}{fileExtension}";
                var filePath = Path.Combine(uploadsRoot, fileName);
                
                using (var stream = System.IO.File.Create(filePath))
                {
                    await photo.CopyToAsync(stream);
                }
                user.PhotoUrl = $"/uploads/{fileName}";
            }

            await _context.SaveChangesAsync();
            return Ok(user);
        }
    }
}



