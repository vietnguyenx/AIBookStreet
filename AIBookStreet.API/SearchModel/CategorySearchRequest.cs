﻿namespace AIBookStreet.API.SearchModel
{
    public class CategorySearchRequest
    {
        public string? CategoryName { get; set; }
        public Guid? AuthorId { get; set; }
    }
}
