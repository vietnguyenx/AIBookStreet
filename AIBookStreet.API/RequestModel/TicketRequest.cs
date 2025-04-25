using AIBookStreet.Repositories.Data.Entities;

namespace AIBookStreet.API.RequestModel
{
    public class TicketRequest
    {
        public Guid Id { get; set; }
        public string TicketCode { get; set; } = null!;
        public string SecretPasscode { get; set; } = null!;
        public virtual EventRegistrationRequest? EventRegistration { get; set; }
    }
}
