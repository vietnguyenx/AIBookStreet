﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class CheckAttendModel
    {
        public Guid Id { get; set; }
        public string? TicketCode { get; set; }
        public bool IsAttended { get; set; }
    }
}
