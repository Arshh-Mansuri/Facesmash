using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FacesmashAPI.Data;
using FacesmashAPI.Models;
using BCrypt.Net;

namespace FacesmashAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password" });

            // Set session data
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.Name);

            return Ok(new
            {
                userId = user.Id,
                name = user.Name,
                email = user.Email,
                message = "Login successful"
            });
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || 
                string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Gender))
            {
                return BadRequest(new { message = "Name, email, password, and gender are required" });
            }

            var existing = await _context.Users.AnyAsync(u => u.Email == request.Email);
            if (existing)
            {
                return Conflict(new { message = "Email already in use" });
            }

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Gender = request.Gender,
                PhotoUrl = "https://via.placeholder.com/300x300/cccccc/666666?text=No+Photo",
                Bio = null,
                Rating = 1200
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Set session data
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.Name);

            return Ok(new
            {
                userId = user.Id,
                name = user.Name,
                email = user.Email,
                message = "Account created successfully"
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized(new { message = "Not logged in" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new
            {
                userId = user.Id,
                name = user.Name,
                email = user.Email,
                photoUrl = user.PhotoUrl,
                bio = user.Bio,
                rating = user.Rating
            });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class SignupRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Gender { get; set; } // "M" or "F"
    }
}