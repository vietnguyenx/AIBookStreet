namespace AIBookStreet.API.RequestModel
{
    public class UserRoleRequest 
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public DateTime? AssignedAt { get; set; }
        public bool? IsApproved { get; set; }
    }
}
