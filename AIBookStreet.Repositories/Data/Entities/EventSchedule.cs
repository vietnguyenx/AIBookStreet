using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class EventSchedule : BaseEntity
    {
        public DateOnly EventDate {  get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public Guid? EventId { get; set; }

        public virtual Event? Event { get; set; }
    }
}
