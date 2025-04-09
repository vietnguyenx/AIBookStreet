using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class StoreSchedule : BaseEntity
    {
        public Guid StoreId { get; set; }
        public virtual Store Store { get; set; }
        
        public DayOfWeek DayOfWeek { get; set; } // 0-6: Chủ nhật đến thứ 7
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
        public bool IsClosed { get; set; } 
        public DateTime? SpecialDate { get; set; } 
    }
} 