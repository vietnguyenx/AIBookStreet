using AIBookStreet.Repositories.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class BookStoreModel : BaseModel
    {
        public string BookStoreName { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime? OpeningTime { get; set; }
        public DateTime? ClosingTime { get; set; }

        public Guid? ManagerId { get; set; }
        public UserModel? Manager { get; set; }

        public Guid? ZoneId { get; set; }
        public ZoneModel? Zone { get; set; }

        public IList<InventoryModel>? Inventories { get; set; }
    }
}
