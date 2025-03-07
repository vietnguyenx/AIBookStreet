using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class SouvenirModel
    {
        public Guid? Id { get; set; }
        public string SouvenirName { get; set; }
        public string? Description { get; set; }
        public string? BaseImgUrl { get; set; }
        public decimal? Price { get; set; }
    }
}
