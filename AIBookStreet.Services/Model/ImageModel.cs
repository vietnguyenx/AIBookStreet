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
        public Guid? Id { get; set; }
        public string Url { get; set; } = null!;
        public string? Type { get; set; }
        public string AltText { get; set; } = null!;
        public Guid? EntityId { get; set; }
    }
}
