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
using Microsoft.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Net.Mime.MediaTypeNames;

namespace AIBookStreet.Repositories.Repositories.Repositories.Repository
{
    public class OrderRepository(BSDbContext context) : BaseRepository<Order>(context), IOrderRepository
    {
        private readonly BSDbContext _context = context;
        public async Task<List<Order>?> GetAllNotPagination(List<Guid>? orderIds, decimal? minAmount, decimal? maxAmount, string? paymentMethod, string? status, DateTime? startDate, DateTime? endDate, Guid? storeId)
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
                if (storeId != null && storeId.HasValue)
                {
                    queryable = queryable.Where(o => o.StoreId <= storeId.Value);
                }
            }

            var orders = await queryable.ToListAsync();
            return orders;
        }
        public async Task<(List<Order>, long)> GetAllPagination(List<Guid>? orderIds, decimal? minAmount, decimal? maxAmount, string? paymentMethod, string? status, DateTime? startDate, DateTime? endDate, Guid? storeId, int? pageNumber, int? pageSize, string? sortField, int? sortOrder)
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
                if (storeId != null && storeId.HasValue)
                {
                    queryable = queryable.Where(o => o.StoreId <= storeId.Value);
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
                                        .ThenInclude(b => b.Images)
                                    .Include(o => o.OrderDetails)
                                     .ThenInclude(od => od.Inventory)
                                        .ThenInclude(i => i.Souvenir)
                                  .SingleOrDefaultAsync();

            return order;
        }
         public async Task<(List<object>?, List<object>?, int, decimal?)> GetStoreStaticsByDate(DateTime? date, Guid storeId)
        {
            date = date != null ? date : new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            var queryable = _context.Orders.Where(o => o.CreatedDate.Year == date.Value.Year && o.CreatedDate.Month == date.Value.Month && o.CreatedDate.Day == date.Value.Day && !o.IsDeleted && o.StoreId == storeId);
            var startTime = new TimeOnly( 8, 0, 0);
            var endTime = new TimeOnly( 22, 59, 59);
            var orderResult = new List<object>();
            var amountResult = new List<object>();
            decimal? totalProfit = 0;
            for(var i = 0;i<14;i++)
            {
                var orderCount = await queryable.CountAsync(o => o.CreatedDate.Hour == startTime.Hour);
                var amounts = await queryable.Where(o => o.CreatedDate.Hour == startTime.Hour).ToListAsync();
                decimal? totalAmount = 0;
                if (amounts != null && amounts.Count > 0)
                {
                    foreach (var amount in amounts)
                    {
                        totalAmount += amount.TotalAmount;
                    }
                }
                orderResult.Add(new
                {
                    label = startTime.ToString("HH:mm") + " - " + startTime.AddHours(1).ToString("HH:mm"),
                    value = orderCount
                });
                totalProfit += totalAmount;
                amountResult.Add(new
                {
                    label = startTime.ToString("HH:mm") + " - " + startTime.AddHours(1).ToString("HH:mm"),
                    value = totalAmount
                });
                startTime = startTime.AddHours(1);
            }
            var totalOrigin = await queryable.CountAsync();

            return (orderResult, amountResult, totalOrigin, totalProfit);
        }
        public async Task<(List<object>?, List<object>?, int, decimal?)> GetStoreStaticsByMonth(int? month, int? year, Guid storeId)
        {
            var monthData = DateTime.Now.Month;
            var yearData = DateTime.Now.Year;
            if (month != null)
            {
                monthData = month.Value;
            }
            if (year != null)
            {
                yearData = year.Value;
            }
            var queryable = _context.Orders.Where(o => o.CreatedDate.Year == year && o.CreatedDate.Month == month && !o.IsDeleted && o.StoreId == storeId);
            var startTime = new DateTime(yearData, monthData, 1, 0, 0, 0);
            var endTime = startTime.AddMonths(1).AddSeconds(-1);
            var orderResult = new List<object>();
            var amountResult = new List<object>();
            decimal? totalProfit = 0;
            while (startTime.Month == monthData && startTime.Year == yearData)
            {
                var orderCount = await queryable.CountAsync(o => o.CreatedDate.Day == startTime.Day);
                var amounts = await queryable.Where(o => o.CreatedDate.Day == startTime.Day).ToListAsync();
                decimal? totalAmount = 0;
                if (amounts != null && amounts.Count > 0)
                {
                    foreach (var amount in amounts)
                    {
                        totalAmount += amount.TotalAmount;
                    }
                }
                orderResult.Add(new
                {
                    label = startTime.ToString("dd"),
                    value = orderCount
                });
                totalProfit += totalAmount;
                amountResult.Add(new
                {
                    label = startTime.ToString("dd"),
                    value = totalAmount
                });
                startTime = startTime.AddDays(1);
            }
            var totalOrigin = await queryable.CountAsync();

            return (orderResult, amountResult, totalOrigin, totalProfit);
        }
        public async Task<(List<object>?, List<object>?, int, decimal?)> GetStoreStaticsByYear(int? year, Guid storeId)
        {
            var yearData = DateTime.Now.Year;
            if (year != null)
            {
                yearData = year.Value;
            }
            var queryable = _context.Orders.Where(o => o.CreatedDate.Year == year && !o.IsDeleted && o.StoreId == storeId);
            var startTime = new DateTime(yearData, 1, 1, 0, 0, 0);
            var endTime = startTime.AddYears(1).AddSeconds(-1);
            var orderResult = new List<object>();
            var amountResult = new List<object>();
            decimal? totalProfit = 0;
            while (startTime.Year == yearData)
            {
                var orderCount = await queryable.CountAsync(o => o.CreatedDate.Month == startTime.Month);
                var amounts = await queryable.Where(o => o.CreatedDate.Month == startTime.Month).ToListAsync();
                decimal? totalAmount = 0;
                if (amounts != null && amounts.Count > 0)
                {
                    foreach (var amount in amounts)
                    {
                        totalAmount += amount.TotalAmount;
                    }
                }
                orderResult.Add(new
                {
                    label = startTime.ToString("MM"),
                    value = orderCount
                });
                totalProfit += totalAmount;
                amountResult.Add(new
                {
                    label = startTime.ToString("MM"),
                    value = totalAmount
                });
                startTime = startTime.AddMonths(1);
            }
            var totalOrigin = await queryable.CountAsync();

            return (orderResult, amountResult, totalOrigin, totalProfit);
        }
    }
}
