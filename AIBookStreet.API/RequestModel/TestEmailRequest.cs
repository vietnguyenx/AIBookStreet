namespace AIBookStreet.API.RequestModel
{
    public class TestEmailRequest
    {
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime? DOB { get; set; }
        public string? Gender { get; set; }
        public string TemporaryPassword { get; set; } = null!;
        public string? BaseImgUrl { get; set; }
        public string? RequestedRoleName { get; set; }
    }
} 