using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System;
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

        // Send a message
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            message.SentAt = DateTime.Now;
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Sent successfully", messageId = message.Id });
        }

        // Get messages between two users
        [HttpGet("between/{user1Id}/{user2Id}")]
        public async Task<IActionResult> GetConversation(int user1Id, int user2Id)
        {
            var messages = await _context.Messages
                .Where(m =>
                    (m.SenderId == user1Id && m.ReceiverId == user2Id) ||
                    (m.SenderId == user2Id && m.ReceiverId == user1Id))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return Ok(messages);
        }

        //Delete message by ID
        [HttpDelete("{id}")]
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
