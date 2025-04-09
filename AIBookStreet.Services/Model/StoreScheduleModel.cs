using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class StoreScheduleModel : BaseModel
    {
        public Guid StoreId { get; set; }
        public StoreModel Store { get; set; }
        
        public DayOfWeek DayOfWeek { get; set; }
        public string OpenTime { get; set; }
        public string CloseTime { get; set; }
        public bool IsClosed { get; set; }
        public DateTime? SpecialDate { get; set; }
    }
} 