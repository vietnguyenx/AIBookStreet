using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Data;
using AIBookStreet.Repositories.Repositories.Base;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AIBookStreet.Repositories.Repositories.Repositories.Repository
{
    public class OrderRepository(BSDbContext context) : BaseRepository<Order>(context), IOrderRepository
    {
        public async Task<List<Order>> GetAllNotPagination(List<Guid?> orderIds, decimal? minAmount, decimal? maxAmount, string? paymentMethod, string? status, DateTime? startDate, DateTime? endDate)
        {
            var queryable = GetQueryable();
            queryable = queryable.Where(o => !o.IsDeleted);
            if (queryable.Any())
            {
                if (orderIds != null && orderIds.Count > 0)
                {
                    queryable = queryable.Where(o => orderIds.Contains(o.Id));
                }
                if (minAmount != null && minAmount > 0)
                {
                    queryable = queryable.Where(o => o.TotalAmount >= minAmount);
                }
                if (maxAmount != null && maxAmount > 0)
                {
                    queryable = queryable.Where(o => o.TotalAmount <= maxAmount);
                }
                if (!string.IsNullOrEmpty(paymentMethod))
                {
                    queryable = queryable.Where(o => o.PaymentMethod.ToLower().Equals(paymentMethod.ToLower()));
                }
                if (!string.IsNullOrEmpty(status))
                {
                    queryable = queryable.Where(o => o.Status != null && o.Status.ToLower().Equals(status.ToLower()));
                }
                if (startDate.HasValue)
                {
                    queryable = queryable.Where(o => o.LastUpdatedDate >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    queryable = queryable.Where(o => o.LastUpdatedDate <= endDate.Value);
                }
            }

            var orders = await queryable.ToListAsync();
            return orders;
        }
        public async Task<(List<Order>, long)> GetAllPagination(List<Guid?> orderIds, decimal? minAmount, decimal? maxAmount, string? paymentMethod, string? status, DateTime? startDate, DateTime? endDate, int? pageNumber, int? pageSize, string? sortField, int? sortOrder)
        {
            var queryable = GetQueryable();
            string field = string.IsNullOrEmpty(sortField) ? "CreatedDate" : sortField;
            var order = sortOrder != null && sortOrder == -1;
            queryable = order ? base.ApplySort(queryable, field, -1) : base.ApplySort(queryable, field, 1);
            if (queryable.Any())
            {
                if (orderIds != null && orderIds.Count > 0)
                {
                    queryable = queryable.Where(o => orderIds.Contains(o.Id));
                }
                if (minAmount != null && minAmount > 0)
                {
                    queryable = queryable.Where(o => o.TotalAmount >= minAmount);
                }
                if (maxAmount != null && maxAmount > 0)
                {
                    queryable = queryable.Where(o => o.TotalAmount <= maxAmount);
                }
                if (!string.IsNullOrEmpty(paymentMethod))
                {
                    queryable = queryable.Where(o => o.PaymentMethod.ToLower().Equals(paymentMethod.ToLower()));
                }
                if (!string.IsNullOrEmpty(status))
                {
                    queryable = queryable.Where(o => !string.IsNullOrEmpty(o.Status) && o.Status.ToLower().Equals(status.ToLower()));
                }
                if (startDate.HasValue)
                {
                    queryable = queryable.Where(o => o.LastUpdatedDate >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    queryable = queryable.Where(o => o.LastUpdatedDate <= endDate.Value);
                }
            }
            var totalOrigin = queryable.Count();

            pageNumber = pageNumber == null ? 1 : pageNumber;
            pageSize = pageSize == null ? 10 : pageSize;

            queryable = GetQueryablePagination(queryable, (int)pageNumber, (int)pageSize);

            var orders = await queryable.ToListAsync();

            return (orders, totalOrigin);
        }
        public async Task<Order?> GetByID(Guid id)
        {
            var query = GetQueryable(o => o.Id == id);
            var order = await query.Include(o => o.OrderDetails)
                                    .ThenInclude(od => od.Inventory)
                                        .ThenInclude(i => i.Store)
                                    .Include(o => o.OrderDetails)
                                     .ThenInclude(od => od.Inventory)
                                        .ThenInclude(i => i.Book)
                                    .Include(o => o.OrderDetails)
                                     .ThenInclude(od => od.Inventory)
                                        .ThenInclude(i => i.Souvenir)
                                  .SingleOrDefaultAsync();

            return order;
        }
    }
}
