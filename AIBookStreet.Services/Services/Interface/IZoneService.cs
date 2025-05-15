using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IZoneService
    {
        Task<Zone?> AddAZone(ZoneModel model);
        Task<(long, Zone?)> UpdateAZone(Guid? id, ZoneModel model);
        Task<(long, Zone?)> DeleteAZone(Guid id);
        Task<Zone?> GetAZoneById(Guid id);
        Task<List<Zone>?> GetAllActiveZones();
        Task<(List<Zone>?, long)> GetAllZonesPagination(string? key, Guid? streetID, int? pageNumber, int? pageSize, string? sortField, bool? desc);
        Task<List<Zone>?> GetAllByStreetId(Guid streetID);
    }
}
