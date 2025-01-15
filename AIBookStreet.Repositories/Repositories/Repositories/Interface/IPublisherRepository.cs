using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface IPublisherRepository : IBaseRepository<Publisher>
    {
        Task<List<Publisher>> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<Publisher?> GetById(Guid id);
        Task<(List<Publisher>, long)> Search(Publisher publisher, int pageNumber, int pageSize, string sortField, int sortOrder);
        
    }
}
