using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class UserStore : BaseEntity
    {
        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        public Guid StoreId { get; set; }
        public virtual Store Store { get; set; }

        // store rent info
        public DateTime StartDate { get; set; }  
        public DateTime? EndDate { get; set; }   
        public string? Status { get; set; }     //Active, Terminated, Expired
        public string? ContractNumber { get; set; } // hop dong thue (neu co)
        public string? ContractFileUrl { get; set; } // URL của file hợp đồng
        public string? Notes { get; set; }
    }
}
