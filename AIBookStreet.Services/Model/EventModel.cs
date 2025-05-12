using AIBookStreet.Repositories.Data.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class EventModel
    {
        public string EventName { get; set; } = null!;
        public string OrganizerEmail { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public IFormFile? BaseImgFile { get; set; }
        public IFormFile? VideoFile { get; set; }
        public List<IFormFile>? OtherImgFile { get; set; }
        public bool IsOpen { get; set; }
        public bool AllowAds { get; set; }
        public Guid? ZoneId { get; set; }
    }
}
