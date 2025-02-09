using AIBookStreet.Repositories.Data.Entities;
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
        public Guid? Id { get; set; }
        [Required]
        [MinLength(2, ErrorMessage = "Name is too short")]
        [MaxLength(100, ErrorMessage = "Name is too long")]
        public string AuthorName { get; set; } = null!;
        public DateTime? DOB { get; set; }
        public string? Nationality { get; set; }
        public string? Biography { get; set; }
    }
}
