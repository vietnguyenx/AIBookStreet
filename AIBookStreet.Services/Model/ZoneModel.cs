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
        public Guid? Id { get; set; }
        [Required]
        public string ZoneName { get; set; } = null!;
        public string? Description { get; set; }

        public Guid? StreetId { get; set; }
    }
}
