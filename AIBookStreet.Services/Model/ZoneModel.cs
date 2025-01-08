using AIBookStreet.Repositories.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class ZoneModel : BaseModel
    {
        public string ZoneName { get; set; }
        public string? Description { get; set; }

        public Guid? StreetId { get; set; }
        public StreetModel? Street { get; set; }

        public IList<BookStoreModel>? BookStores { get; set; }
    }
}
