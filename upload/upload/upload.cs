using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.Runtime.InteropServices;

namespace upload
{
   
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly string connectionString = "Data Source=uploads.db";
        private readonly string blobConnection = "DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=userupload123;AccountKey=5g8lvX+NlC8YGmb7FcmHPSUKZr6JnTcWIP5bj7QqCiDjBr6UbUnn8ZCkOGMZe0bsP+eZ3q8wS/OX+AStC526iQ==;BlobEndpoint=https://userupload123.blob.core.windows.net/;FileEndpoint=https://userupload123.file.core.windows.net/;QueueEndpoint=https://userupload123.queue.core.windows.net/;TableEndpoint=https://userupload123.table.core.windows.net/";
        private readonly string containerName = "useruploads";

        [HttpPost("photo")]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            // 1️⃣ Upload to Azure Blob
            var blobServiceClient = new BlobServiceClient(blobConnection);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            string blobName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var blobClient = containerClient.GetBlobClient(blobName);
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream);
            }

            string blobUrl = blobClient.Uri.ToString();

            using (var connection = new SqliteConnection("Data Source=uploads.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"CREATE TABLE IF NOT EXISTS Photos (
                                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            FileName TEXT,
                                            Url TEXT,
                                            UploadDate TEXT
                );";
                command.ExecuteNonQuery();

                var insert = connection.CreateCommand();
                insert.CommandText = @"
                INSERT INTO Photos (FileName, Url, UploadDate)
                VALUES ($file, $url, $date);";
                insert.Parameters.AddWithValue("$file", "example.jpg");
                insert.Parameters.AddWithValue("$url", "https://example.com/example.jpg");
                insert.Parameters.AddWithValue("$date", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                insert.ExecuteNonQuery();
            }

            // 2️⃣ Store metadata in SQLite
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                    @"CREATE TABLE IF NOT EXISTS Photos (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FileName TEXT,
                    Url TEXT,
                    UploadDate TEXT
                );";
                command.ExecuteNonQuery();

                var insert = connection.CreateCommand();
                insert.CommandText = "INSERT INTO Photos (FileName, Url, UploadDate) VALUES ($file, $url, $date)";
                insert.Parameters.AddWithValue("$file", file.FileName);
                insert.Parameters.AddWithValue("$url", blobUrl);
                insert.Parameters.AddWithValue("$date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                insert.ExecuteNonQuery();
            }

            return Ok(new { message = "Uploaded!", url = blobUrl });
        }
    }

}
