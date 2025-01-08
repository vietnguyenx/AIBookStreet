using AIBookStreet.Repositories.Data.Entities;

namespace AIBookStreet.API.RequestModel
{
    public class BookStoreRequest : BaseRequest
    {
        public string BookStoreName { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime? OpeningTime { get; set; }
        public DateTime? ClosingTime { get; set; }

        public Guid? ManagerId { get; set; }

        public Guid? ZoneId { get; set; }

    }
}
