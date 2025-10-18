using FacesmashAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacesmashAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaderboardController : ControllerBase
    {
        private readonly AppDbContext _db;

        public LeaderboardController(AppDbContext db)
        {
            _db = db;
        }

        // GET api/leaderboard
        [HttpGet]
        public async Task<IActionResult> GetTop100()
        {
            var leaderboard = await _db.Users
                .OrderByDescending(u => u.Rating)
                .Take(100) // top 100
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.PhotoUrl,
                    u.Bio,
                    u.Rating
                })
                .ToListAsync();

            return Ok(leaderboard);
        }
    }
}
