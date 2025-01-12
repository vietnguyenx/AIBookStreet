using AIBookStreet.Repositories.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class BookCategoryModel
    {
        [Required]
        public Guid BookId { get; set; }
        [Required]
        public Guid CategoryId { get; set; }
    }
}
