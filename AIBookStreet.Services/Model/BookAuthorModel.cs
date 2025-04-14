using AIBookStreet.Repositories.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class BookAuthorModel
    {
        [Required]
        public Guid BookId { get; set; }
        public BookModel? Book { get; set; }
        [Required]
        public Guid AuthorId { get; set; }
        public AuthorModel? Author { get; set; }

        public string? AuthorName { get; set; }
        public bool IsNewAuthor { get; set; } = false;
    }
}
