using AIBookStreet.Repositories.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class CategoryModel
    {
        [Required]
        public string CategoryName { get; set; } = null!;
        public string? Description { get; set; }
    }
}
