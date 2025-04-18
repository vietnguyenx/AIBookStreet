using AIBookStreet.Repositories.Data.Entities;

namespace AIBookStreet.API.RequestModel
{
    public class TicketRequest
    {
        public Guid Id { get; set; }
        public string TicketCode { get; set; }
        public string SecretPasscode { get; set; }
        public virtual EventRegistrationRequest? EventRegistration { get; set; }
    }
}
