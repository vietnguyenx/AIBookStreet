using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface IOrderDetailRepository : IBaseRepository<OrderDetail>
    {
        Task<OrderDetail?> GetByID(Guid id);
        Task<List<OrderDetail>?> GetCart(Guid storeId);
        Task<List<OrderDetail>?> GetAllOrderDetail(List<Guid>? storeIds, Guid? orderId, Guid? storeId, Guid? entityId);
    }
}
