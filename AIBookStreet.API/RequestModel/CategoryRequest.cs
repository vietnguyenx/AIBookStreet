namespace AIBookStreet.API.RequestModel
{
    public class CategoryRequest
    {
        public Guid Id { get; set; }
        public string CategoryName { get; set; } = null!;
        public string? Description { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
