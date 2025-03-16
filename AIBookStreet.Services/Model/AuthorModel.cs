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
    public class AuthorModel
    {
        public string AuthorName { get; set; } = null!;
        public DateTime? DOB { get; set; }
        public string? Nationality { get; set; }
        public string? Biography { get; set; }
        public IFormFile? ImgFile { get; set; }
    }
}
