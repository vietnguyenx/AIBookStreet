using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.UnitOfWork.Interface;
using AIBookStreet.Services.Base;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AIBookStreet.Services.Common.ConstantMessage;

namespace AIBookStreet.Services.Services.Service
{
    public class OrderService(IUnitOfWork repository, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseService<Order>(mapper, repository, httpContextAccessor), IOrderService
    {
        private readonly IUnitOfWork _repository = repository;
        public async Task<(long, Order?)> AddAnOrder(OrderModel model)
        {
            try
            {
                var orderDetails = await _repository.OrderDetailRepository.GetCart(model.StoreId);
                decimal totalAmount = 0;
                var order = new Order
                {
                    PaymentMethod = model.PaymentMethod
                };
                var setOrder = await SetBaseEntityToCreateFunc(order);
                if (orderDetails != null)
                {
                    foreach (var orderDetail in orderDetails)
                    {
                        var inventory = await _repository.InventoryRepository.GetByID(orderDetail.InventoryId);
                        if (inventory != null && inventory.Book != null)
                        {
                            if (inventory.Quantity < orderDetail.Quantity)
                            {
                                return (1, null); //sl trong inventory khong du
                            }
                            else
                            {
                                inventory.Quantity -= orderDetail.Quantity;
                                totalAmount += (decimal)inventory.Book.Price * orderDetail.Quantity;
                                if (!(await _repository.InventoryRepository.Update(inventory)))
                                {
                                    return (3, null);
                                }
                            }
                        }
                        if (inventory != null && inventory.Souvenir != null)
                        {
                            if (inventory.Quantity < orderDetail.Quantity)
                            {
                                return (1, null); //sl trong inventory khong du
                            }
                            else
                            {
                                inventory.Quantity -= orderDetail.Quantity;
                                totalAmount += (decimal)inventory.Souvenir.Price * orderDetail.Quantity;
                                if (!(await _repository.InventoryRepository.Update(inventory)))
                                {
                                    return (3, null);
                                }
                            }

                        }
                        orderDetail.OrderId = setOrder.Id;
                        orderDetail.LastUpdatedDate = DateTime.Now;
                        await _repository.OrderDetailRepository.Update(orderDetail);
                    }
                }
                
                setOrder.TotalAmount = totalAmount;
                
                var isSuccess = await _repository.OrderRepository.Add(setOrder);

                return isSuccess ? (2, setOrder) : (3, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Order?> GetAnOrderById(Guid orderId)
        {
            return await _repository.OrderRepository.GetByID(orderId);
        }
        public async Task<(List<Order>?, long)> GetPaginationOrders(decimal? minAmount, decimal? maxAmount, string? paymentMethod, string? status, DateOnly? startDate, DateOnly? endDate, int? pageNumber, int? pageSize, string? sortField, int? sortOrder)
        {
            try
            {
                var user = await GetUserInfo();
                var userStores = await _repository.UserStoreRepository.GetByUserId(user.Id);
                var orderIds = new List<Guid?>();
                if (userStores != null )
                {
                    var storeIds = new List<Guid>();
                    foreach ( var us in userStores)
                    {
                        storeIds.Add(us.StoreId);
                    }
                    if (storeIds != null)
                    {
                        foreach ( var storeId in storeIds)
                        {
                            var orderDetails = await _repository.OrderDetailRepository.GetAllOrderDetail(null, null, storeId, null);
                            if (orderDetails != null)
                            {
                                foreach( var orderDetail in orderDetails)
                                {
                                    if (orderDetail.OrderId != null)
                                    {
                                        orderIds.Add(orderDetail.OrderId);
                                    }
                                }
                            }
                        }
                    }

                }
                DateTime? startDateTime = startDate.HasValue ? startDate.Value.ToDateTime(TimeOnly.MinValue) : null;
                DateTime? endDateTime = endDate.HasValue ? endDate.Value.ToDateTime(TimeOnly.MaxValue) : null;
                var orders = await _repository.OrderRepository.GetAllPagination(orderIds, minAmount, maxAmount, paymentMethod, status, startDateTime, endDateTime, pageNumber, pageSize, sortField, sortOrder);
                return orders.Item1.Count > 0 ? (orders.Item1, orders.Item2) : (null, 0);
            } catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<Order>?> GetAllOrders(decimal? minAmount, decimal? maxAmount, string? paymentMethod, string? status, DateOnly? startDate, DateOnly? endDate)
        {
            try
            {
                var user = await GetUserInfo();
                var userStores = await _repository.UserStoreRepository.GetByUserId(user.Id);
                var orderIds = new List<Guid?>();
                if (userStores != null)
                {
                    var storeIds = new List<Guid>();
                    foreach (var us in userStores)
                    {
                        storeIds.Add(us.StoreId);
                    }
                    if (storeIds != null)
                    {
                        foreach (var storeId in storeIds)
                        {
                            var orderDetails = await _repository.OrderDetailRepository.GetAllOrderDetail(null, null, storeId, null);
                            if (orderDetails != null)
                            {
                                foreach (var orderDetail in orderDetails)
                                {
                                    if (orderDetail.OrderId != null)
                                    {
                                        orderIds.Add(orderDetail.OrderId);
                                    }
                                }
                            }
                        }
                    }

                }
                DateTime? startDateTime = startDate.HasValue ? startDate.Value.ToDateTime(TimeOnly.MinValue) : null;
                DateTime? endDateTime = endDate.HasValue ? endDate.Value.ToDateTime(TimeOnly.MaxValue) : null;
                var orders = await _repository.OrderRepository.GetAllNotPagination(orderIds, minAmount, maxAmount, paymentMethod, status, startDateTime, endDateTime);
                return orders.Count > 0 ? orders : null;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
