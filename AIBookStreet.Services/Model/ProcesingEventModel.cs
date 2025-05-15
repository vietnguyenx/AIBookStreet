using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class ProcesingEventModel
    {
        public bool IsApprove { get; set; }
        public string? Message { get; set; }
    }
}
