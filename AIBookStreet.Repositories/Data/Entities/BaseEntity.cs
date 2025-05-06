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
        [Key]
        public Guid Id { get; set; }

        public string? CreatedBy { get; set; }

        private DateTime _createdDate;
        public DateTime CreatedDate
        {
            get => _createdDate.ToLocalTime();
            set => _createdDate = value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : value;
        }

        public string? LastUpdatedBy { get; set; }

        private DateTime? _lastUpdatedDate;
        public DateTime? LastUpdatedDate
        {
            get => _lastUpdatedDate?.ToLocalTime();
            set => _lastUpdatedDate = value.HasValue ? (value.Value.Kind == DateTimeKind.Local ? value.Value.ToUniversalTime() : value.Value) : null;
        }

        public bool IsDeleted { get; set; }
    }
}
