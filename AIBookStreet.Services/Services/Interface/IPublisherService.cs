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
        Task<(PublisherModel?, string)> Add(PublisherModel publisherModel);
        Task<(PublisherModel?, string)> Update(PublisherModel publisherModel);
        Task<(PublisherModel?, string)> Delete(Guid publisherId);
        Task<long> GetTotalCount();
    }
}
