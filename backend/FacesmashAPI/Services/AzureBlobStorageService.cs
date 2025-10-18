using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace FacesmashAPI.Services
{
    /// <summary>
    /// Service for handling Azure Blob Storage operations for photo uploads
    /// </summary>
    public class AzureBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        private readonly string _baseUrl;

        public AzureBlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureStorage") ?? 
                                 configuration["AzureStorage:ConnectionString"];
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Azure Storage connection string is not configured.");
            }

            _blobServiceClient = new BlobServiceClient(connectionString);
            _containerName = configuration["AzureStorage:ContainerName"] ?? "profile-photos";
            _baseUrl = configuration["AzureStorage:BaseUrl"] ?? "https://profileurl.blob.core.windows.net";
        }

        /// <summary>
        /// Uploads a photo to Azure Blob Storage
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="userId">The user ID for organizing the blob</param>
        /// <returns>The URL of the uploaded blob</returns>
        public async Task<string> UploadPhotoAsync(IFormFile file, int userId)
        {
            try
            {
                // Ensure container exists
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName);
                if (string.IsNullOrWhiteSpace(fileExtension))
                {
                    fileExtension = file.ContentType switch
                    {
                        "image/jpeg" => ".jpg",
                        "image/png" => ".png",
                        "image/webp" => ".webp",
                        _ => ".jpg"
                    };
                }

                var fileName = $"users/{userId}/{Guid.NewGuid():N}{fileExtension}";
                var blobClient = containerClient.GetBlobClient(fileName);

                // Set content type
                var blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                };

                // Upload the file
                using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders
                });

                // Return the public URL
                return $"{_baseUrl}/{_containerName}/{fileName}";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to upload photo to Azure Blob Storage: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deletes a photo from Azure Blob Storage
        /// </summary>
        /// <param name="blobUrl">The URL of the blob to delete</param>
        /// <returns>True if deletion was successful</returns>
        public async Task<bool> DeletePhotoAsync(string blobUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(blobUrl) || !blobUrl.Contains(_baseUrl))
                {
                    return false;
                }

                // Extract blob name from URL
                var uri = new Uri(blobUrl);
                var blobName = uri.AbsolutePath.TrimStart('/');
                
                // Remove container name from blob name
                if (blobName.StartsWith($"{_containerName}/"))
                {
                    blobName = blobName.Substring(_containerName.Length + 1);
                }

                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                var response = await blobClient.DeleteIfExistsAsync();
                return response.Value;
            }
            catch (Exception ex)
            {
                // Log the error but don't throw - deletion failure shouldn't break the app
                Console.WriteLine($"Failed to delete blob {blobUrl}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets a list of all photos for a specific user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>List of photo URLs</returns>
        public async Task<List<string>> GetUserPhotosAsync(int userId)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var photos = new List<string>();

                await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: $"users/{userId}/"))
                {
                    photos.Add($"{_baseUrl}/{_containerName}/{blobItem.Name}");
                }

                return photos;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get user photos: {ex.Message}", ex);
            }
        }
    }
}
