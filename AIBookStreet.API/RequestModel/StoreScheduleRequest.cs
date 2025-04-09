using System;

namespace AIBookStreet.API.RequestModel
{
    public class StoreScheduleRequest : BaseRequest
    {
        public Guid StoreId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public string OpenTime { get; set; } // Format: "HH:mm:ss"
        public string CloseTime { get; set; } // Format: "HH:mm:ss"
        public bool IsClosed { get; set; }
        public DateTime? SpecialDate { get; set; }
    }
} 