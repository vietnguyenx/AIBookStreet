﻿using AIBookStreet.Repositories.Data.Entities;

namespace AIBookStreet.API.RequestModel
{
    public class TicketRequest
    {
        public Guid Id { get; set; }
        public string TicketCode { get; set; } = null!;
        public string SecretPasscode { get; set; } = null!;
        public List<EventRegistrationRequest>? EventRegistrations { get; set; }
        public DateTime IssuedAt { get; set; }
    }
}
