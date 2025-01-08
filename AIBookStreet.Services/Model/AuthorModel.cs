using AIBookStreet.Repositories.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class AuthorModel : BaseModel
    {
        public string AuthorName { get; set; }
        public DateTime? DOB { get; set; }
        public string? Nationality { get; set; }
        public string? Biography { get; set; }

        public IList<BookAuthorModel>? BookAuthors { get; set; }
    }
}
