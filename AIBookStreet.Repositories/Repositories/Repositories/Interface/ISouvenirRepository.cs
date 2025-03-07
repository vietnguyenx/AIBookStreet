using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface ISouvenirRepository : IBaseRepository<Souvenir>
    {
        Task<(List<Souvenir>, long)> GetAllPagination(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc);
        Task<Souvenir?> GetByID(Guid? id);
        Task<(List<Souvenir>, long)> GetAllPaginationForAdmin(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc);
    }
}
