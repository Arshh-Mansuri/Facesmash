using FacesmashAPI.Data;
using FacesmashAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FacesmashAPI.Controllers
{
    /// <summary>
    /// Controller for handling private messaging between users.
    /// Provides functionality to send messages and retrieve conversation threads.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _databaseContext;

        /// <summary>
        /// Initializes a new instance of the MessagesController.
        /// </summary>
        /// <param name="databaseContext">The database context for data access.</param>
        public MessagesController(AppDbContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        /// <summary>
        /// Sends a private message from one user to another.
        /// </summary>
        /// <param name="messageRequest">Contains sender, recipient, and message content.</param>
        /// <returns>Confirmation of message sent with message details.</returns>
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest messageRequest)
        {
            // Validate input parameters
            if (messageRequest.FromUserId <= 0 || messageRequest.ToUserId <= 0)
            {
                return BadRequest(new { message = "Valid sender and recipient user IDs are required." });
            }

            if (string.IsNullOrWhiteSpace(messageRequest.Content))
            {
                return BadRequest(new { message = "Message content cannot be empty." });
            }

            if (messageRequest.FromUserId == messageRequest.ToUserId)
            {
                return BadRequest(new { message = "Cannot send message to yourself." });
            }

            // Validate message length
            if (messageRequest.Content.Length > 1000)
            {
                return BadRequest(new { message = "Message content cannot exceed 1000 characters." });
            }

            try
            {
                // Verify both users exist
                var senderExists = await _databaseContext.Users.AnyAsync(user => user.Id == messageRequest.FromUserId);
                var recipientExists = await _databaseContext.Users.AnyAsync(user => user.Id == messageRequest.ToUserId);

                if (!senderExists || !recipientExists)
                {
                    return NotFound(new { message = "One or both users not found." });
                }

                // Create new message
                var newMessage = new Message
                {
                    FromUserId = messageRequest.FromUserId,
                    ToUserId = messageRequest.ToUserId,
                    Content = messageRequest.Content.Trim(),
                    SentAt = DateTime.UtcNow
                };

                // Save message to database
                _databaseContext.Messages.Add(newMessage);
                await _databaseContext.SaveChangesAsync();

                return Ok(new 
                { 
                    message = "Message sent successfully.",
                    messageId = newMessage.Id,
                    fromUserId = newMessage.FromUserId,
                    toUserId = newMessage.ToUserId,
                    content = newMessage.Content,
                    sentAt = newMessage.SentAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error sending message.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves the conversation thread between two users.
        /// Returns all messages exchanged between the specified users in chronological order.
        /// </summary>
        /// <param name="userId">ID of the first user in the conversation.</param>
        /// <param name="peerId">ID of the second user in the conversation.</param>
        /// <returns>List of messages in the conversation thread.</returns>
        [HttpGet("thread")]
        public async Task<IActionResult> GetConversationThread([FromQuery] int userId, [FromQuery] int peerId)
        {
            // Validate input parameters
            if (userId <= 0 || peerId <= 0)
            {
                return BadRequest(new { message = "Valid user IDs are required." });
            }

            if (userId == peerId)
            {
                return BadRequest(new { message = "Cannot retrieve conversation with yourself." });
            }

            try
            {
                // Retrieve conversation messages between the two users
                var conversationMessages = await _databaseContext.Messages
                    .Where(message => 
                        (message.FromUserId == userId && message.ToUserId == peerId) || 
                        (message.FromUserId == peerId && message.ToUserId == userId))
                    .OrderBy(message => message.SentAt)
                    .Select(message => new
                    {
                        message.Id,
                        message.FromUserId,
                        message.ToUserId,
                        message.Content,
                        message.SentAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    userId = userId,
                    peerId = peerId,
                    messageCount = conversationMessages.Count,
                    messages = conversationMessages
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving conversation thread.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all conversations for a specific user.
        /// Returns a list of users that the specified user has exchanged messages with.
        /// </summary>
        /// <param name="userId">ID of the user whose conversations to retrieve.</param>
        /// <returns>List of conversation partners with latest message info.</returns>
        [HttpGet("conversations")]
        public async Task<IActionResult> GetUserConversations([FromQuery] int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new { message = "Valid user ID is required." });
            }

            try
            {
                // Get all unique conversation partners for the user
                var conversations = await _databaseContext.Messages
                    .Where(message => message.FromUserId == userId || message.ToUserId == userId)
                    .GroupBy(message => message.FromUserId == userId ? message.ToUserId : message.FromUserId)
                    .Select(group => new
                    {
                        partnerId = group.Key,
                        latestMessage = group.OrderByDescending(m => m.SentAt).First(),
                        messageCount = group.Count()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    userId = userId,
                    conversationCount = conversations.Count,
                    conversations = conversations.Select(conv => new
                    {
                        partnerId = conv.partnerId,
                        latestMessage = new
                        {
                            conv.latestMessage.Id,
                            conv.latestMessage.FromUserId,
                            conv.latestMessage.ToUserId,
                            conv.latestMessage.Content,
                            conv.latestMessage.SentAt
                        },
                        messageCount = conv.messageCount
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving conversations.", error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Request model for sending a message.
    /// Contains all necessary information to create a new message.
    /// </summary>
    public class SendMessageRequest
    {
        /// <summary>
        /// ID of the user sending the message.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Sender user ID must be valid.")]
        public int FromUserId { get; set; }

        /// <summary>
        /// ID of the user receiving the message.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Recipient user ID must be valid.")]
        public int ToUserId { get; set; }

        /// <summary>
        /// Content of the message. Cannot be empty or exceed 1000 characters.
        /// </summary>
        [Required]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Message content must be between 1 and 1000 characters.")]
        public string Content { get; set; } = string.Empty;
    }
}


