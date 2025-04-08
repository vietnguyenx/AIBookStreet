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
    public class OrderDetailRepository(BSDbContext context) : BaseRepository<OrderDetail>(context), IOrderDetailRepository
    {
        public async Task<List<OrderDetail>?> GetCart(Guid storeId)
        {
            var queryable = GetQueryable();
            return await queryable.Include(od => od.Inventory)
                                            .ThenInclude(i => i.Souvenir)
                                        .Include(od => od.Inventory)
                                            .ThenInclude(i => i.Book)
                                            .Where(od => od.Inventory.StoreId == storeId && od.OrderId == null).ToListAsync();
        }
        public async Task<List<OrderDetail>?> GetAllOrderDetail(List<Guid>? storeIds, Guid? orderId, Guid? storeId, Guid? entityId)
        {
            var queryable = GetQueryable();
            queryable = queryable.Include(od => od.Inventory);
            if (queryable.Any())
            {
                if (storeIds != null && storeIds.Count > 0)
                {
                    queryable = queryable.Where(od => storeIds.Contains(od.Inventory.StoreId));
                }
                if (orderId == null)
                {
                    queryable = queryable.Where(od => od.OrderId == orderId);
                }
                if (storeId == null)
                {
                    queryable = queryable.Where(od => od.Inventory.StoreId == storeId);
                }
                if (entityId == null)
                {
                    queryable = queryable.Where(od => od.Inventory.EntityId == entityId);
                }
            }
            return await queryable.ToListAsync();
        }
        public async Task<OrderDetail?> GetByID(Guid id)
        {
            var query = GetQueryable(o => o.Id == id);
            var orderDetail = await query.Include(od => od.Inventory)
                                            .ThenInclude(i => i.Store)
                                        .Include(od => od.Inventory)
                                            .ThenInclude(i => i.Souvenir)
                                        .Include(od => od.Inventory)
                                            .ThenInclude(i => i.Book)
                                  .SingleOrDefaultAsync();

            return orderDetail;
        }
    }
}
