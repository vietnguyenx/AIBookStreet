using AIBookStreet.API.RequestModel;

namespace AIBookStreet.API.ResponseModel
{
    public class TicketResponse
    {
        public Guid Id { get; set; }
        public string TicketCode { get; set; } = null!;
        public string SecretPasscode { get; set; } = null!;
        public EventRegistrationResponse? EventRegistration { get; set; }
        public DateTime IssuedAt { get; set; }
    }
}
