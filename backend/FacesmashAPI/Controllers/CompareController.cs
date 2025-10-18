using FacesmashAPI.Data;
using FacesmashAPI.Models;
using FacesmashAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacesmashAPI.Controllers
{
    /// <summary>
    /// Controller for handling user comparison and voting functionality.
    /// Manages the core Facesmash feature where users vote between two profiles.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CompareController : ControllerBase
    {
        private readonly AppDbContext _databaseContext;

        /// <summary>
        /// Initializes a new instance of the CompareController.
        /// </summary>
        /// <param name="databaseContext">The database context for data access.</param>
        public CompareController(AppDbContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        /// <summary>
        /// Retrieves the current user's ID from the session.
        /// </summary>
        /// <returns>The user ID if authenticated, null otherwise.</returns>
        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        /// <summary>
        /// Checks if the current user is authenticated.
        /// </summary>
        /// <returns>True if user is logged in, false otherwise.</returns>
        private bool IsUserAuthenticated()
        {
            return GetCurrentUserId() != null;
        }

        /// <summary>
        /// Processes a vote between two users and updates their ELO ratings.
        /// Requires user authentication.
        /// </summary>
        /// <param name="voteRequest">Contains the IDs of the winner and loser.</param>
        /// <returns>Updated ratings for both users or an error message.</returns>
        [HttpPost("vote")]
        public async Task<IActionResult> ProcessVote([FromBody] VoteRequest voteRequest)
        {
            // Validate authentication
            if (!IsUserAuthenticated())
            {
                return Unauthorized(new { message = "Authentication required to vote." });
            }

            // Validate request data
            if (voteRequest.WinnerId <= 0 || voteRequest.LoserId <= 0)
            {
                return BadRequest(new { message = "Valid user IDs are required." });
            }

            if (voteRequest.WinnerId == voteRequest.LoserId)
            {
                return BadRequest(new { message = "Winner and loser must be different users." });
            }

            // Retrieve users from database
            var winner = await _databaseContext.Users.FindAsync(voteRequest.WinnerId);
            var loser = await _databaseContext.Users.FindAsync(voteRequest.LoserId);

            if (winner == null || loser == null)
            {
                return NotFound(new { message = "One or both users not found." });
            }

            try
            {
                // Calculate new ratings using ELO system
                var (winnerNewRating, loserNewRating) = EloService.CalculateMatchResult(
                    winner.Rating, 
                    loser.Rating
                );

                // Update user ratings
                winner.Rating = winnerNewRating;
                loser.Rating = loserNewRating;

                // Save changes to database
                await _databaseContext.SaveChangesAsync();

                return Ok(new 
                { 
                    message = "Vote processed successfully.",
                    winnerRating = winner.Rating, 
                    loserRating = loser.Rating,
                    ratingChange = new
                    {
                        winnerChange = winnerNewRating - winner.Rating,
                        loserChange = loserNewRating - loser.Rating
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error processing vote.", error = ex.Message });
            }
        }

        /// <summary>
        /// Debug endpoint to retrieve all users and their information.
        /// Useful for development and troubleshooting.
        /// </summary>
        /// <returns>List of all users with their details.</returns>
        [HttpGet("debug")]
        public async Task<IActionResult> GetAllUsersForDebug()
        {
            try
            {
                var users = await _databaseContext.Users.ToListAsync();
                
                return Ok(new
                {
                    totalUsers = users.Count,
                    users = users.Select(user => new
                    {
                        user.Id,
                        user.Name,
                        user.Email,
                        user.Gender,
                        user.PhotoUrl,
                        user.Rating,
                        user.CreatedAt
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving users.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves two random users for comparison.
        /// Used by the frontend to display voting options.
        /// </summary>
        /// <returns>Two random users with their profile information.</returns>
        [HttpGet("random")]
        public async Task<IActionResult> GetRandomUsersForComparison()
        {
            try
            {
                // Check if we have enough users
                var totalUserCount = await _databaseContext.Users.CountAsync();
                
                if (totalUserCount < 2)
                {
                    return BadRequest(new 
                    { 
                        message = "Not enough users for comparison.", 
                        totalUsers = totalUserCount 
                    });
                }

                // Fetch all users and select two randomly
                var allUsers = await _databaseContext.Users.ToListAsync();
                var randomUsers = allUsers
                    .OrderBy(user => Guid.NewGuid())
                    .Take(2)
                    .ToList();

                return Ok(randomUsers.Select(user => new
                {
                    user.Id,
                    user.Name,
                    user.PhotoUrl,
                    user.Rating,
                    user.Bio,
                    user.Gender
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving random users.", error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Request model for vote submission.
    /// Contains the IDs of the winner and loser in a comparison.
    /// </summary>
    public class VoteRequest
    {
        /// <summary>
        /// ID of the user who won the comparison.
        /// </summary>
        public int WinnerId { get; set; }

        /// <summary>
        /// ID of the user who lost the comparison.
        /// </summary>
        public int LoserId { get; set; }
    }
}
