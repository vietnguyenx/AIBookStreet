namespace AIBookStreet.API.RequestModel
{
    public class CategoryRequest : BaseRequest
    {
        public string CategoryName { get; set; }
        public string? Description { get; set; }
    }
}
