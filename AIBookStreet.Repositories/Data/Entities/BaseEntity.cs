using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public abstract class BaseEntity
    {
        private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);

        [Key]
        public Guid Id { get; set; }

        public string? CreatedBy { get; set; }

        private DateTime _createdDate;
        public DateTime CreatedDate
        {
            get => _createdDate;
            set => _createdDate = value.Kind == DateTimeKind.Utc ? value.Add(VietnamOffset) : value;
        }

        public string? LastUpdatedBy { get; set; }

        private DateTime? _lastUpdatedDate;
        public DateTime? LastUpdatedDate
        {
            get => _lastUpdatedDate;
            set => _lastUpdatedDate = value.HasValue ? (value.Value.Kind == DateTimeKind.Utc ? value.Value.Add(VietnamOffset) : value.Value) : null;
        }

        public bool IsDeleted { get; set; }
    }
}
