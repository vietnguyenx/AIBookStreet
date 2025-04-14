using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace AIBookStreet.Services.Services.Service
{
    public class FirebaseStorageService : IFirebaseStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;
        private readonly ILogger<FirebaseStorageService> _logger;

        public FirebaseStorageService(IOptions<FirebaseSettings> settings, ILogger<FirebaseStorageService> logger)
        {
            var credential = GoogleCredential.FromFile(settings.Value.ServiceAccountKeyPath);
            _storageClient = StorageClient.Create(credential);
            _bucketName = settings.Value.Bucket ?? throw new ArgumentNullException(nameof(settings.Value.Bucket));
            _logger = logger;
        }

        public string GetFileUrl(string fileName)
        {
            return $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{Uri.EscapeDataString(fileName)}?alt=media";
        }

        public async Task<string> UploadFileAsync(IFormFile file, string? customFileName = null)
        {
            try
            {
                await ValidateFileAsync(file);

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                // Generate unique filename if not provided
                var fileName = customFileName ?? $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

                // Upload to Firebase
                var obj = await _storageClient.UploadObjectAsync(
                    _bucketName,
                    fileName,
                    file.ContentType,
                    stream
                );

                return GetFileUrl(obj.Name);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload file: {ex.Message}");
            }
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                {
                    return;
                }

                var uri = new Uri(fileUrl);
                
                // Check if this is a Firebase Storage URL
                if (!uri.Host.Contains("firebasestorage.googleapis.com"))
                {
                    _logger.LogWarning($"Skipping deletion of non-Firebase Storage URL: {fileUrl}");
                    return;
                }

                // Extract the file name from the Firebase Storage URL
                // URL format: https://firebasestorage.googleapis.com/v0/b/{bucket}/o/{fileName}?alt=media
                var segments = uri.Segments;
                if (segments.Length < 4)
                {
                    throw new ArgumentException("Invalid Firebase Storage URL format");
                }
                
                // The file name is the 4th segment (index 3)
                var fileName = Uri.UnescapeDataString(segments[3]);
                await _storageClient.DeleteObjectAsync(_bucketName, fileName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete file: {ex.Message}");
            }
        }

        private async Task ValidateFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            // Check file size (max 10MB)
            if (file.Length > 10 * 1024 * 1024)
                throw new ArgumentException("File size exceeds 10MB limit");

            // Validate file type
            var allowedTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/gif",
                "video/mp4",
                "video/mpeg",
                "video/quicktime"
            };

            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                throw new ArgumentException("File type not supported");

            // Additional validation can be added here
        }
    }
} 