﻿using Microsoft.AspNetCore.Http;

namespace AIBookStreet.API.RequestModel
{
    public class BookRequest : BaseRequest
    {
        public string ISBN { get; set; } = null!;
        public string? Title { get; set; }
        public DateTime? PublicationDate { get; set; }
        public decimal? Price { get; set; }
        public string? Languages { get; set; }
        public string? Description { get; set; }
        public string? Size { get; set; }
        public string? Status { get; set; }
        public IFormFile? MainImageFile { get; set; }
        public List<IFormFile>? AdditionalImageFiles { get; set; }

        public Guid? PublisherId { get; set; }

        public List<Guid>? AuthorIds { get; set; }
        public List<Guid>? CategoryIds { get; set; }
    }
}
