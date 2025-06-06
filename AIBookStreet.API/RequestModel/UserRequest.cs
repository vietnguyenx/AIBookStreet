﻿namespace AIBookStreet.API.RequestModel
{
    public class UserRequest : BaseRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string? FullName { get; set; }
        public DateTime? DOB { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public Guid? RequestedRoleId { get; set; }
        public IFormFile? MainImageFile { get; set; }
        public List<IFormFile>? AdditionalImageFiles { get; set; }
    }
}
