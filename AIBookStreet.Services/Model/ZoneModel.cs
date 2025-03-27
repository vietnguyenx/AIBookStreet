using AIBookStreet.Repositories.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class ZoneModel
    {
        public string ZoneName { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public Guid? StreetId { get; set; }
    }
}
