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
        private readonly ConcurrentQueue<Guid?> _queue = new();
        public void Enqueue(Guid? message) => _queue.Enqueue(message);
        public bool TryDequeue(out Guid? message) => _queue.TryDequeue(out message);
    }
}
