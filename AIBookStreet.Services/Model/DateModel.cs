using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class DateModel(DateOnly eventDate)
    {
        public DateOnly EventDate { get; set; } = eventDate;
    }
}
