﻿using AIBookStreet.Repositories.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IEventRegistrationQueueService
    {
        void Enqueue(Guid? ticketId);
        bool TryDequeue(out Guid? ticketId);
    }
}
