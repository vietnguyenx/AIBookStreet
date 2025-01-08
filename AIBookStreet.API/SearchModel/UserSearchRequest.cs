namespace AIBookStreet.API.SearchModel
{
    public class UserSearchRequest
    {
        public string UserName { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public DateTime? DOB { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Gender { get; set; }
    }
}
