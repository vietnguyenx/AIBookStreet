using AIBookStreet.Repositories.Data.Entities;

namespace AIBookStreet.API.RequestModel
{
    public class AuthorRequest
    {
        public Guid Id { get; set; }
        public string AuthorName { get; set; } = null!;
        public DateTime? DOB { get; set; }
        public string? Nationality { get; set; }
        public string? Biography { get; set; }
        public bool IsDeleted { get; set; }
        public virtual ICollection<ImageRequest>? Images { get; set; }
    }
}
