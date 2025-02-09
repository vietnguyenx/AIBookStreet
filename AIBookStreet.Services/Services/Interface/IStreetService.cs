using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IStreetService
    {
        Task<(long, Street?)> AddAStreet(StreetModel model);
        Task<(long, Street?)> UpdateAStreet(Guid? id, StreetModel model);
        Task<(long, Street?)> DeleteAStreet(Guid id);
        Task<Street?> GetAStreetById(Guid id);
        Task<List<Street>?> GetAllActiveStreets();
        Task<(List<Street>?, long)> GetAllStreetsPagination(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc);
    }
}
