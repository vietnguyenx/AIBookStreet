using AIBookStreet.Repositories.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class StreetModel : BaseModel
    {
        public string StreetName { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }

        public IList<ZoneModel>? Zones { get; set; }
        public IList<EventModel>? Events { get; set; }
    }
}
