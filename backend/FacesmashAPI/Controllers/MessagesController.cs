using FacesmashAPI.Data;
using FacesmashAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacesmashAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public MessagesController(AppDbContext db)
        {
            _db = db;
        }

        // POST api/messages
        [HttpPost]
        public async Task<IActionResult> Send([FromBody] SendMessageRequest req)
        {
            if (req.FromUserId <= 0 || req.ToUserId <= 0 || string.IsNullOrWhiteSpace(req.Content))
                return BadRequest("fromUserId, toUserId and content are required");

            // Optional: validate users exist
            if (!await _db.Users.AnyAsync(u => u.Id == req.FromUserId) || !await _db.Users.AnyAsync(u => u.Id == req.ToUserId))
                return NotFound("One or both users not found");

            var message = new Message
            {
                FromUserId = req.FromUserId,
                ToUserId = req.ToUserId,
                Content = req.Content.Trim(),
                SentAt = DateTime.UtcNow
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();
            return Ok(new { message.Id, message.FromUserId, message.ToUserId, message.Content, message.SentAt });
        }

        // GET api/messages/thread?userId=1&peerId=2
        [HttpGet("thread")]
        public async Task<IActionResult> GetThread([FromQuery] int userId, [FromQuery] int peerId)
        {
            if (userId <= 0 || peerId <= 0) return BadRequest("userId and peerId are required");

            var msgs = await _db.Messages
                .Where(m => (m.FromUserId == userId && m.ToUserId == peerId) || (m.FromUserId == peerId && m.ToUserId == userId))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return Ok(msgs);
        }
    }

    public class SendMessageRequest
    {
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public string Content { get; set; }
    }
}


