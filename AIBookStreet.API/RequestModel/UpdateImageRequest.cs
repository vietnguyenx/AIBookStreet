using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AIBookStreet.API.RequestModel
{
    public class UpdateImageRequest
    {
        [Required]
        public IFormFile File { get; set; } = null!;
        public string? Type { get; set; }
        public string? AltText { get; set; }
        public Guid? EntityId { get; set; }
    }
} 