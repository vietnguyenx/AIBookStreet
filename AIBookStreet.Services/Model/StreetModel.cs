using AIBookStreet.Repositories.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class StreetModel
    {
        [Required]
        public required string StreetName { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
    }
}
