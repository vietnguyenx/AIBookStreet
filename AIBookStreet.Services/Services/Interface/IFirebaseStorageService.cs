using AIBookStreet.Services.Model;
using Microsoft.AspNetCore.Http;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IFirebaseStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string? customFileName = null);
        Task DeleteFileAsync(string fileUrl);
        string GetFileUrl(string fileName);
        Task<(byte[] fileData, string contentType, string fileName)> DownloadFileAsync(string fileUrl);
    }
} 