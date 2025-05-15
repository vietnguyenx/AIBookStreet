using AIBookStreet.Services.Model;
using System.ComponentModel.DataAnnotations;

namespace AIBookStreet.API.RequestModel
{
    public class CreateEventRequest
    {
        public EventModel EventModel { get; set; }
        [Required(ErrorMessage = "Sự kiện phải diễn ra ít nhất 1 ngày.")]
        public required List<string> EventDates { get; set; }
        public required List<string> StartTimes { get; set; }
        public required List<string> EndTimes { get; set; }
    }
}
