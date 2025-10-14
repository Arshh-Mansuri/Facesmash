using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FacesmashAPI.Data;
using FacesmashAPI.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FacesmashAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UploadController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("photo")]
        public async Task<IActionResult> UploadPhoto(IFormFile file, int userId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            try
            {
                // Get Azure configuration
                var connectionString = _configuration["AzureBlobStorage:ConnectionString"];
                var containerName = _configuration["AzureBlobStorage:ContainerName"];

                // Upload to Azure Blob Storage
                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // Ensure container exists
                await containerClient.CreateIfNotExistsAsync();

                // Generate unique blob name
                string blobName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var blobClient = containerClient.GetBlobClient(blobName);

                // Upload file to blob storage
                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                string blobUrl = blobClient.Uri.ToString();

                // Update user's photo URL in database
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.PhotoUrl = blobUrl;
                    await _context.SaveChangesAsync();
                }

                return Ok(new { 
                    message = "Photo uploaded successfully!", 
                    url = blobUrl,
                    userId = userId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Upload failed", details = ex.Message });
            }
        }

        [HttpGet("photos/{userId}")]
        public async Task<IActionResult> GetUserPhotos(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found");

            return Ok(new { 
                userId = user.Id,
                photoUrl = user.PhotoUrl,
                name = user.Name
            });
        }
    }
}
