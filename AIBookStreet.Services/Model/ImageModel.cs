using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class ImageModel
    {
        [Required]
        public required string Url { get; set; }
        public string? Type { get; set; }
        [Required]
        public string AltText { get; set; } = null!;
        public Guid? EntityId { get; set; }
    }
}
