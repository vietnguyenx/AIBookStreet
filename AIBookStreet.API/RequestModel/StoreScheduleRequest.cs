using System;

namespace AIBookStreet.API.RequestModel
{
    public class StoreScheduleRequest : BaseRequest
    {
        public Guid StoreId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
        public bool IsClosed { get; set; }
        public DateTime? SpecialDate { get; set; }
    }
} 