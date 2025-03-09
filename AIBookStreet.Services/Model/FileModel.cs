using Microsoft.AspNetCore.Http;

namespace AIBookStreet.Services.Model
{
    public class FileModel
    {
        public IFormFile File { get; set; } = null!;
        public string? Type { get; set; }
        public string? AltText { get; set; }
        public Guid? EntityId { get; set; }
    }
} 