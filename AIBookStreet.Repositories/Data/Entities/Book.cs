using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class Book : BaseEntity
    {
        public string ISBN { get; set; } // preferred
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public DateTime? PublicationDate { get; set; }
        public int? PageCount { get; set; }
        public string? PrintType { get; set; } // e.g. BOOK, MAGAZINE
        public decimal? Price { get; set; }
        public string? Languages { get; set; }
        public string? Description { get; set; }
        public string? MaturityRating { get; set; } // e.g. NOT_MATURE
        public string? ContentVersion { get; set; }

        public string? Size { get; set; } // bạn có thể giữ lại nếu dùng để phân loại thêm
        public string? Status { get; set; }

        public string? ThumbnailUrl { get; set; } // hình ảnh chính (nên đổi tên rõ hơn)
        public string? PreviewLink { get; set; } // link xem trước
        public string? InfoLink { get; set; } // link chi tiết trên Google Books

        public Guid? PublisherId { get; set; }
        public virtual Publisher? Publisher { get; set; }

        public virtual ICollection<BookAuthor>? BookAuthors { get; set; }
        public virtual ICollection<Inventory>? Inventories { get; set; }
        public virtual ICollection<BookCategory>? BookCategories { get; set; }
        public virtual ICollection<Image>? Images { get; set; }
    }

}
