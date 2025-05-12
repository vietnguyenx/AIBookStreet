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
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? BaseImgUrl { get; set; }
        public string? VideoLink { get; set; }
        public bool IsOpen { get; set; }
        public bool AllowAds { get; set; }

        public Guid? ZoneId { get; set; }
        public virtual Zone? Zone { get; set; }
        public virtual ICollection<Image>? Images { get; set; }
        public virtual ICollection<EventRegistration>? EventRegistrations { get; set; }
    }
}
