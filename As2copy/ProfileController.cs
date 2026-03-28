using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FacesmashAPI.Data;
using FacesmashAPI.Models;

namespace FacesmashAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProfileController(AppDbContext context)
        {
            _context = context;
        }

        // Get all users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // Get user by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found");

            return Ok(user);
        }

        // Create new user
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            if (user == null)
                return BadRequest("User data is required");

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // Update user
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User updatedUser)
        {
            if (id != updatedUser.Id)
                return BadRequest("ID mismatch");

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found");

            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;
            user.Gender = updatedUser.Gender;
            user.Rating = updatedUser.Rating;
            // Note: PasswordHash should be handled separately for security

            try
            {
                await _context.SaveChangesAsync();
                return Ok(user);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("User was modified by another user");
            }
        }

        // Delete user
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok("User deleted successfully");
        }

        // Update user rating
        [HttpPut("{id}/rating")]
        public async Task<IActionResult> UpdateRating(int id, [FromBody] int newRating)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found");

            user.Rating = newRating;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Rating updated", userId = id, newRating = newRating });
        }
    }
}