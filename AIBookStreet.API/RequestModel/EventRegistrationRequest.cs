namespace AIBookStreet.API.RequestModel
{
    public class EventRegistrationRequest
    {
        public Guid Id { get; set; }
        public string RegistrantName { get; set; } = null!;
        public string? RegistrantEmail { get; set; }
        public string RegistrantPhoneNumber { get; set; } = null!;
        public string RegistrantAgeRange { get; set; } = null!;
        public string RegistrantGender { get; set; } = null!;
        public string? RegistrantAddress { get; set; }
        public string? ReferenceSource { get; set; }
        public bool HasAttendedBefore { get; set; }
        public bool IsAttended { get; set; }
        public string? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public EventRequest? Event { get; set; }
    }
}
