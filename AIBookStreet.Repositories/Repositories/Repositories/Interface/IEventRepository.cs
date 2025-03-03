using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface IEventRepository : IBaseRepository<Event>
    {
        Task<(List<Event>, long)> GetAllPagination(string? key,DateTime? start, DateTime? end, Guid? streetID, int? pageNumber, int? pageSize, string? sortField, bool? desc);
        Task<(List<Event>, long)> GetAllPaginationForAdmin(string? key, DateTime? start, DateTime? end, Guid? streetID, int? pageNumber, int? pageSize, string? sortField, bool? desc);
        Task<Event?> GetByID(Guid? id);
        Task<List<Event>?> GetEventsComing(int number);
        Task<List<DateOnly>?> GetDatesInMonth(int month);
        Task<List<Event>?> GetByDate(DateTime date);
    }
}
