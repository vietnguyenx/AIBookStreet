namespace AIBookStreet.API.SearchModel
{
    public class StoreSearchRequest
    {
        public string? StoreName { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? StoreTheme { get; set; }
        public string? Type { get; set; }
        public Guid? ZoneId { get; set; }
    }
}
