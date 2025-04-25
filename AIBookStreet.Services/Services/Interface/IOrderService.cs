using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IOrderService
    {
        Task<(long, Order?, string?)> AddAnOrder(OrderModel model);
        Task<(long, Order?, string?)> ConfirmOrder(Guid id);
        Task<(long, Order?, string?)> CancelAnOrder(Guid id);
        Task<Order?> GetAnOrderById(Guid orderId);
        Task<(List<Order>?, long)> GetPaginationOrders(decimal? minAmount, decimal? maxAmount, string? paymentMethod, string? status, DateTime? startDate, DateTime? endDate, Guid? storeId, int? pageNumber, int? pageSize, string? sortField, int? sortOrder);
        Task<List<Order>?> GetAllOrders(decimal? minAmount, decimal? maxAmount, string? paymentMethod, string? status, DateTime? startDate, DateTime? endDate, Guid? storeId);
        Task<(List<object>?, List<object>?, int, decimal?)> GetAllStoreStaticsByDate(DateTime? date);
        Task<(List<object>?, List<object>?, int, decimal?)> GetAllStoreStaticsByMonth(int? month, int? year);
        Task<(List<object>?, List<object>?, int, decimal?)> GetAllStoreStaticsByYear(int? year);
        Task<(List<object>?, List<object>?, int, decimal?)> GetStoreStaticsByDate(DateTime? date, Guid storeId);
        Task<(List<object>?, List<object>?, int, decimal?)> GetStoreStaticsByMonth(int? month, int? year, Guid storeId);
        Task<(List<object>?, List<object>?, int, decimal?)> GetStoreStaticsByYear(int? year, Guid storeId);
    }
}
