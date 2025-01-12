using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface IZoneRepository : IBaseRepository<Zone>
    {
        Task<List<Zone>> GetAll();
        Task<(List<Zone>, long)> GetAllPagination(string? key, Guid? streetID, int? pageNumber, int? pageSize, string? sortField, bool? desc);
        Task<Zone?> GetByID(Guid id);
    }
}
