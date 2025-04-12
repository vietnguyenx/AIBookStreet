using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class PersonModel : BaseModel
    {
        public int ExternalId { get; set; }
        public string Gender { get; set; } = null!;
        public string Features { get; set; } = null!;
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
        public int DailyAppearances { get; set; }
        public int TotalAppearances { get; set; }
        public DateTime ExternalCreatedAt { get; set; }
        public DateTime ExternalUpdatedAt { get; set; }
    }
} 