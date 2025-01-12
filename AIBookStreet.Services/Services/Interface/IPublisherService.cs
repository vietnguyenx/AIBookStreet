using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IPublisherService
    {
        public Task<List<PublisherModel>> GetAll();
        public Task<List<PublisherModel>> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder);
        public Task<PublisherModel> GetById(Guid id);
        public Task<(List<PublisherModel>?, long)> Search(PublisherModel publisherModel, int pageNumber, int pageSize, string sortField, int sortOrder);
        public Task<bool> Add(PublisherModel model);
        public Task<bool> Update(PublisherModel publisherModel);
        public Task<bool> Delete(Guid id);
        
        public Task<long> GetTotalCount();
    }
}
