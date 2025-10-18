using FacesmashAPI.Data;
using FacesmashAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacesmashAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompareController : ControllerBase
    {
        private readonly AppDbContext _db;
        private const int K = 24; // K-factor for Elo

        public CompareController(AppDbContext db)
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

        // POST api/compare/vote - Requires authentication
        [HttpPost("vote")]
        public async Task<IActionResult> Vote([FromBody] VoteRequest request)
        {
            if (!IsAuthenticated())
                return Unauthorized(new { message = "Must be logged in to vote" });

            var winner = await _db.Users.FindAsync(request.WinnerId);
            var loser = await _db.Users.FindAsync(request.LoserId);

            if (winner == null || loser == null)
                return BadRequest("Invalid user IDs");

            // Calculate expected scores
            double expectedWinner = 1.0 / (1.0 + Math.Pow(10, (loser.Rating - winner.Rating) / 400.0));
            double expectedLoser = 1.0 / (1.0 + Math.Pow(10, (winner.Rating - loser.Rating) / 400.0));

            // Update ratings
            winner.Rating = (int)(winner.Rating + K * (1 - expectedWinner));
            loser.Rating = (int)(loser.Rating + K * (0 - expectedLoser));

            await _db.SaveChangesAsync();

            return Ok(new { WinnerRating = winner.Rating, LoserRating = loser.Rating });
        }

        // GET api/compare/debug - Debug endpoint to check users
        [HttpGet("debug")]
        public async Task<IActionResult> DebugUsers()
        {
            var users = await _db.Users.ToListAsync();
            return Ok(new
            {
                TotalUsers = users.Count,
                Users = users.Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Gender,
                    u.PhotoUrl,
                    u.Rating
                })
            });
        }

        // GET api/compare/random
        [HttpGet("random")]
        public async Task<IActionResult> GetRandomUsers()
        {
            // Debug: Check total user count
            var totalUsers = await _db.Users.CountAsync();
            
            // Fetch all users into memory, then pick 2 randomly
            var users = (await _db.Users
                .ToListAsync())
                .OrderBy(u => Guid.NewGuid())
                .Take(2)
                .ToList();

            if (users.Count < 2)
                return BadRequest($"Not enough users. Total users in database: {totalUsers}");

            return Ok(users.Select(u => new
            {
                u.Id,
                u.Name,
                u.PhotoUrl,
                u.Rating,
                u.Bio,
                u.Gender // Include gender for display if needed
            }));
        }
    }

    // Request model
    public class VoteRequest
    {
        public int WinnerId { get; set; }
        public int LoserId { get; set; }
    }
}
