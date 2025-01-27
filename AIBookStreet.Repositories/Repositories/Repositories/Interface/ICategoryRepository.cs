using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface ICategoryRepository : IBaseRepository<Category>
    {
        Task<List<Category>> GetAll();
        Task<(List<Category>, long)> GetAllPagination(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc);
        Task<(List<Category>, long)> GetAllPaginationForAdmin(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc);
        Task<Category?> GetByID(Guid id);
    }
}
