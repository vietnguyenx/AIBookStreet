using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Common;

namespace AIBookStreet.Services.Model
{
    public class GoogleBookResponseModel
    {
        public string ISBN { get; set; }
        public string? Title { get; set; }
        [JsonConverter(typeof(DateTimeFormatConverter))]
        public DateTime? PublicationDate { get; set; }
        public decimal? Price { get; set; }
        public string? Languages { get; set; }
        public string? Description { get; set; }
        public string? Size { get; set; }
        public string? Status { get; set; }
        public IList<BookAuthorModel>? BookAuthors { get; set; }
        public IList<BookCategoryModel>? BookCategories { get; set; }
        public PublisherModel? Publisher { get; set; }
        public IList<AIBookStreet.Repositories.Data.Entities.Image>? Images { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
} 