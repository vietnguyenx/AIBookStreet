using AIBookStreet.Repositories.Data.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class PublisherModel : BaseModel
    {
        public string PublisherName { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Description { get; set; }
        public string? Website { get; set; }
        public IFormFile? MainImageFile { get; set; }
        public List<IFormFile>? AdditionalImageFiles { get; set; }

        public Guid? ManagerId { get; set; }
        public UserModel? Manager { get; set; }

        public IList<BookModel>? Books { get; set; }
        public IList<Image>? Images { get; set; }
    }
}
