using FacesmashAPI.Data;
using FacesmashAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

        // GET api/profile/{id}
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
                user.Bio
            });
        }

        // ✅ GET api/profile/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            // Without JWT, fallback: require userId query for now (temporary approach)
            if (!int.TryParse(HttpContext.Request.Query["userId"], out int userId))
                return BadRequest("userId is required");

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

        // ✅ PUT api/profile/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
        {
            if (!int.TryParse(HttpContext.Request.Query["userId"], out int userId))
                return BadRequest("userId is required");

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");

            user.Name = request.Name ?? user.Name;
            user.Bio = request.Bio ?? user.Bio;

            await _db.SaveChangesAsync();
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
    }

    public class UpdateProfileRequest
    {
        public string? Name { get; set; }
        public string? Bio { get; set; }
    }
}
