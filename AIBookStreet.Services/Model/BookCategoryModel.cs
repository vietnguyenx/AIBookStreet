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
        public BookModel? Book { get; set; }
        [Required]
        public Guid CategoryId { get; set; }
        public CategoryModel? Category { get; set; }

        public string? CategoryName { get; set; }
        public bool IsNewCategory { get; set; } = false;
    }
}
