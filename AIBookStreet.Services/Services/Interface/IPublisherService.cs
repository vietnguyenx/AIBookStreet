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
        Task<(List<PublisherModel>?, long)> SearchPagination(PublisherModel publisherModel, int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<List<PublisherModel>?> SearchWithoutPagination(PublisherModel publisherModel);
        Task<bool> Add(PublisherModel publisherModel);
        Task<bool> Update(PublisherModel publisherModel);
        Task<bool> Delete(Guid id);
        Task<long> GetTotalCount();
    }
}
