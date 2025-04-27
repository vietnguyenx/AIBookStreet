using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        Task<Order?> GetByID(Guid id);
        Task<(List<Order>?, long)> GetAllPagination(List<Guid>? orderIds, decimal? minAmount, decimal? maxAmount, string? paymentMethod, string? status, DateTime? startDate, DateTime? endDate, Guid? storeId, int? pageNumber, int? pageSize, string? sortField, int? sortOrder);
        Task<List<Order>?> GetAllNotPagination(List<Guid>? orderIds, decimal? minAmount, decimal? maxAmount, string? paymentMethod, string? status, DateTime? startDate, DateTime? endDate, Guid? storeId);
        Task<(List<object>?, List<object>?, int, decimal?)> GetStoreStaticsByDate(DateTime? date, Guid storeId);
        Task<(List<object>?, List<object>?, int, decimal?)> GetStoreStaticsByMonth(int? month, int? year, Guid storeId);
        Task<(List<object>?, List<object>?, int, decimal?)> GetStoreStaticsByYear(int? year, Guid storeId);
        Task<List<Order>> GetAllSortByCreateDate();
        Task<int> GetNumberOrders();
    }
}
