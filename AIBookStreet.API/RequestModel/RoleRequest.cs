namespace AIBookStreet.API.RequestModel
{
    public class RoleRequest : BaseRequest
    {
        public string RoleName { get; set; }
        public string? Description { get; set; }
    }
}
