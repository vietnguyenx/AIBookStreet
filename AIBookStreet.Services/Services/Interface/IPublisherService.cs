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
        Task<List<PublisherModel>> GetAll();
        Task<List<PublisherModel>?> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<PublisherModel?> GetById(Guid id);
        Task<(List<PublisherModel>?, long)> Search(PublisherModel publisherModel, int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<bool> Add(PublisherModel publisherModel);
        Task<bool> Update(PublisherModel publisherModel);
        Task<bool> Delete(Guid id);
        Task<long> GetTotalCount();
    }
}
