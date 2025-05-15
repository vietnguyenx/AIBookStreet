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
        [Required(ErrorMessage = "Vui lòng nhập tên sự kiện")]
        public required string EventName { get; set; }
        public string? Description { get; set; }
        public IFormFile? BaseImgFile { get; set; }
        public IFormFile? VideoFile { get; set; }
        public List<IFormFile>? OtherImgFile { get; set; }
        public bool IsOpen { get; set; }
        public bool AllowAds { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn khu vực tổ chức sự kiện")]
        public Guid ZoneId { get; set; }        
    }
}
