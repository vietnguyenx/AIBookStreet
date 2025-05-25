using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Service
{
    public class ExportEventStatisticQueueService : IExportEventStatisticQueueService
    {
        private readonly ConcurrentQueue<ExportStatisticModel> _queue = new();
        public void Enqueue(ExportStatisticModel model) => _queue.Enqueue(model);
        public bool TryDequeue(out ExportStatisticModel model) => _queue.TryDequeue(out model);
    }
}
