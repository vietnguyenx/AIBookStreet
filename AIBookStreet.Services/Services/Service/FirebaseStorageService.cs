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

                _logger.LogInformation($"Attempting to delete file: {fileUrl}");

                // Handle different URL formats
                string fileName;
                
                try
                {
                    var uri = new Uri(fileUrl);
                    
                    // Check if this is a Firebase Storage URL
                    if (!uri.Host.Contains("firebasestorage"))
                    {
                        _logger.LogWarning($"Skipping deletion of non-Firebase Storage URL: {fileUrl}");
                        return;
                    }

                    // Extract the file name from the URL
                    if (uri.AbsolutePath.Contains("/o/"))
                    {
                        // Standard Firebase URL format: https://firebasestorage.googleapis.com/v0/b/{bucket}/o/{fileName}?alt=media
                        var pathParts = uri.AbsolutePath.Split(new[] { "/o/" }, StringSplitOptions.None);
                        if (pathParts.Length > 1)
                        {
                            fileName = Uri.UnescapeDataString(pathParts[1].Split('?')[0]);
                        }
                        else
                        {
                            throw new ArgumentException($"Could not extract filename from Firebase URL: {fileUrl}");
                        }
                    }
                    else
                    {
                        // Handle alternative URL structures
                        _logger.LogWarning($"Using fallback URL parsing for: {fileUrl}");
                        var segments = uri.Segments;
                        fileName = Uri.UnescapeDataString(segments[segments.Length - 1].Split('?')[0]);
                    }
                }
                catch (UriFormatException ex)
                {
                    _logger.LogError($"Invalid URL format: {fileUrl}, Error: {ex.Message}");
                    throw new ArgumentException($"Invalid URL format: {fileUrl}");
                }

                _logger.LogInformation($"Deleting file with name: {fileName} from bucket: {_bucketName}");
                await _storageClient.DeleteObjectAsync(_bucketName, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to delete file: {fileUrl}, Error: {ex.Message}");
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