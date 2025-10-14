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

        // ✅ GET api/profile/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfile(int id)
        {
            var user = await _db.Users.FindAsync(id);
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

        // ✅ PUT api/profile/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateProfileRequest request)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound("User not found");

            user.Name = request.Name ?? user.Name;
            user.Bio = request.Bio ?? user.Bio;

            await _db.SaveChangesAsync();
            return Ok(user);
        }
    }

    public class UpdateProfileRequest
    {
        public string? Name { get; set; }
        public string? Bio { get; set; }
    }
}
