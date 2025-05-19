using AIBookStreet.Repositories.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IEmailQueueService
    {
        void Enqueue(Ticket message);
        bool TryDequeue(out Ticket message);
    }
}
