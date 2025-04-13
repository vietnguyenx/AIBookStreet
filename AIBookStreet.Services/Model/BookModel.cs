using AIBookStreet.Repositories.Data.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class BookModel : BaseModel
    {
        public string ISBN { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public DateTime? PublicationDate { get; set; }
        public int? PageCount { get; set; }
        public string? PrintType { get; set; }
        public decimal? Price { get; set; }
        public string? Languages { get; set; }
        public string? Description { get; set; }
        public string? MaturityRating { get; set; }
        public string? ContentVersion { get; set; }

        public string? Size { get; set; }
        public string? Status { get; set; }

        public string? ThumbnailUrl { get; set; }
        public string? PreviewLink { get; set; }
        public string? InfoLink { get; set; }

        public IFormFile? MainImageFile { get; set; }
        public List<IFormFile>? AdditionalImageFiles { get; set; }

        public Guid? PublisherId { get; set; }
        public PublisherModel? Publisher { get; set; }

        public IList<BookAuthorModel>? BookAuthors { get; set; }
        public IList<InventoryModel>? Inventories { get; set; }
        public IList<BookCategoryModel>? BookCategories { get; set; }
        public IList<Image>? Images { get; set; }
    }

}
