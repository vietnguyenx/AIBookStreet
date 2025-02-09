using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface IStreetRepository : IBaseRepository<Street>
    {
        Task<List<Street>> GetAll();
        Task<(List<Street>, long)> GetAllPagination(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc);
        Task<(List<Street>, long)> GetAllPaginationForAdmin(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc);
        Task<Street?> GetByID(Guid? id);
        Task<Street?> GetByAddress(string address);
    }
}
