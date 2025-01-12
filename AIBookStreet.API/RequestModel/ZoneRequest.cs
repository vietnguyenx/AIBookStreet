﻿using AIBookStreet.Repositories.Data.Entities;

namespace AIBookStreet.API.RequestModel
{
    public class ZoneRequest
    {
        public Guid Id { get; set; }
        public string ZoneName { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsDeleted { get; set; }
        public virtual Street? Street { get; set; }
    }
}
