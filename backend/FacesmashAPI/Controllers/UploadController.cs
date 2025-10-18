using FacesmashAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacesmashAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly AppDbContext _db;
        private static readonly string[] Allowed = new[] { "image/jpeg", "image/png", "image/webp" };
        private const long MaxBytes = 5 * 1024 * 1024;

        public UploadController(AppDbContext db)
        {
            _db = db;
        }

        // POST api/upload/photo?userId=9
        [HttpPost("photo")]
        public async Task<IActionResult> UploadPhoto([FromQuery] int userId, IFormFile file)
        {
            if (userId <= 0) return BadRequest("userId is required");
            if (file == null || file.Length == 0) return BadRequest("File is required");
            if (file.Length > MaxBytes) return BadRequest("File too large");
            if (!Allowed.Contains(file.ContentType)) return BadRequest("Unsupported file type");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound("User not found");

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "users", userId.ToString());
            Directory.CreateDirectory(dir);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(dir, fileName);

            using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }

            var relativeUrl = $"/uploads/users/{userId}/{fileName}";
            var baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
            var absoluteUrl = baseUrl + relativeUrl;
            user.PhotoUrl = absoluteUrl;
            await _db.SaveChangesAsync();

            return Ok(new { photoUrl = absoluteUrl });
        }
    }
}


