using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IOrderDetailService
    {
        Task<(long,OrderDetail?)> GetByID(Guid id);
        Task<(long, List<OrderDetail>?)> GetCart(Guid storeId);
        Task<List<OrderDetail>?> GetAllOrderDetail(Guid? orderId, Guid? storeId, Guid? entityId);
        Task<OrderDetail?> AddToCart(OrderDetailModel model);
        Task<(long,OrderDetail?)> RemoveFromCart(Guid id);
    }
}
