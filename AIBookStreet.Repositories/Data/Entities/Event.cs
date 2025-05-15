using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class Event : BaseEntity
    {
        public string EventName { get; set; }
        public string? Description { get; set; }
        public string OrganizerEmail { get; set; }
        public string? BaseImgUrl { get; set; }
        public string? VideoLink { get; set; }
        public bool IsOpen { get; set; }
        public bool AllowAds { get; set; }
        public int Version { get; set; }
        public bool? IsApprove { get; set; }
        public string? Message { get; set; }
        public Guid? UpdateForEventId { get; set; }

        public Guid? ZoneId { get; set; }
        public virtual Zone? Zone { get; set; }
        public virtual ICollection<Image>? Images { get; set; }
        public virtual ICollection<EventRegistration>? EventRegistrations { get; set; }
        public virtual ICollection<EventSchedule>? EventSchedules { get; set; }
    }
}
