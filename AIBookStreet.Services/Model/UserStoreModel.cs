﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AIBookStreet.Services.Model
{
    public class UserStoreModel : BaseModel
    {
        public Guid UserId { get; set; }
        public UserModel User { get; set; }

        public Guid StoreId { get; set; }
        public StoreModel Store { get; set; }

        // store rent info
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }     //Active, Terminated, Expired
        public string? ContractNumber { get; set; } // hop dong thue (neu co)
        public IFormFile? ContractFile { get; set; } // File hợp đồng
        public string? ContractFileUrl { get; set; } // URL của file hợp đồng
        public string? Notes { get; set; }
    }
}
