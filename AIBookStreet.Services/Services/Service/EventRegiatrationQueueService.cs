using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Services.Interface;
using Microsoft.AspNetCore.Cors.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Service
{
    public class EventRegiatrationQueueService : IEventRegistrationQueueService
    {
        private readonly ConcurrentQueue<Ticket> _queue = new();
        public void Enqueue(Ticket message) => _queue.Enqueue(message);
        public bool TryDequeue(out Ticket message) => _queue.TryDequeue(out message);
    }
}
