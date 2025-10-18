using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FacesmashAPI.Data;
using FacesmashAPI.Models;

namespace FacesmashAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MessageController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            message.SentAt = DateTime.Now;
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Sent successfully", messageId = message.Id });
        }

        [HttpGet("between/{user1Id:int}/{user2Id:int}")]
        public async Task<IActionResult> GetConversation(int user1Id, int user2Id)
        {
            var messages = await _context.Messages
                .Where(m => (m.SenderId == user1Id && m.ReceiverId == user2Id) ||
                            (m.SenderId == user2Id && m.ReceiverId == user1Id))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return Ok(messages);
        }

        [HttpGet("conversations/{userId:int}")]
        public async Task<IActionResult> GetConversations(int userId)
        {
            try
            {
                // Get all messages for the user
                var userMessages = await _context.Messages
                    .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                    .OrderByDescending(m => m.SentAt)
                    .ToListAsync();

                // Group by other user ID
                var conversationsDict = new Dictionary<int, Message>();
                foreach (var message in userMessages)
                {
                    var otherUserId = message.SenderId == userId ? message.ReceiverId : message.SenderId;
                    if (!conversationsDict.ContainsKey(otherUserId))
                    {
                        conversationsDict[otherUserId] = message;
                    }
                }

                // Get user details
                var otherUserIds = conversationsDict.Keys.ToList();
                var users = await _context.Users
                    .Where(u => otherUserIds.Contains(u.Id))
                    .ToListAsync();

                var result = conversationsDict.Select(kvp => new
                {
                    OtherUser = users.FirstOrDefault(u => u.Id == kvp.Key),
                    LastMessage = kvp.Value,
                    UnreadCount = 0
                }).OrderByDescending(c => c.LastMessage.SentAt);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var msg = await _context.Messages.FindAsync(id);
            if (msg == null) return NotFound();
            _context.Messages.Remove(msg);
            await _context.SaveChangesAsync();
            return Ok("Message deleted");
        }
    }
}





