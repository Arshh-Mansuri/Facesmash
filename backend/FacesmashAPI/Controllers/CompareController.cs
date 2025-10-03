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

        // POST api/compare/vote
        [HttpPost("vote")]
        public async Task<IActionResult> Vote([FromBody] VoteRequest request)
        {
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

        // GET api/compare/males
        [HttpGet("males")]
        public async Task<IActionResult> GetRandomMales()
        {
            var males = await _db.Users
                .Where(u => u.Gender == "M")
                .OrderBy(r => Guid.NewGuid()) // randomize
                .Take(2) // pick 2
                .ToListAsync();

            if (males.Count < 2)
                return BadRequest("Not enough male users");

            return Ok(males.Select(u => new { u.Id, u.Name, u.PhotoUrl, u.Rating }));
        }
    }

    // Request model
    public class VoteRequest
    {
        public int WinnerId { get; set; }
        public int LoserId { get; set; }
    }
}
