namespace AIBookStreet.API.RequestModel
{
    public class EventRegistrationRequest
    {
        public Guid Id { get; set; }
        public string RegistrantName { get; set; }
        public string RegistrantEmail { get; set; }
        public string RegistrantPhoneNumber { get; set; }
        public string RegistrantAgeRange { get; set; }
        public string RegistrantGender { get; set; }
        public string? RegistrantAddress { get; set; }
        public string? ReferenceSource { get; set; }
        public bool HasAttendedBefore { get; set; }
        public EventRequest? Event { get; set; }
    }
}
