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
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                    PaymentMethod = model.PaymentMethod,
                    StoreId = model.StoreId,
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
                //var user = await GetUserInfo();
                //var userStores = await _repository.UserStoreRepository.GetByUserId(user.Id);
                //var orderIds = new List<Guid?>();
                //if (userStores != null )
                //{
                //    var storeIds = new List<Guid>();
                //    foreach ( var us in userStores)
                //    {
                //        storeIds.Add(us.StoreId);
                //    }
                //    if (storeIds != null)
                //    {
                //        foreach ( var storeId in storeIds)
                //        {
                //            var orderDetails = await _repository.OrderDetailRepository.GetAllOrderDetail(null, null, storeId, null);
                //            if (orderDetails != null)
                //            {
                //                foreach( var orderDetail in orderDetails)
                //                {
                //                    if (orderDetail.OrderId != null)
                //                    {
                //                        orderIds.Add(orderDetail.OrderId);
                //                    }
                //                }
                //            }
                //        }
                //    }

                //}
                DateTime? startDateTime = startDate.HasValue ? startDate.Value.ToDateTime(TimeOnly.MinValue) : null;
                DateTime? endDateTime = endDate.HasValue ? endDate.Value.ToDateTime(TimeOnly.MaxValue) : null;
                var orders = await _repository.OrderRepository.GetAllPagination(null, minAmount, maxAmount, paymentMethod, status, startDateTime, endDateTime, pageNumber, pageSize, sortField, sortOrder);
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
                //var user = await GetUserInfo();
                //var userStores = await _repository.UserStoreRepository.GetByUserId(user.Id);
                //var orderIds = new List<Guid?>();
                //if (userStores != null)
                //{
                //    var storeIds = new List<Guid>();
                //    foreach (var us in userStores)
                //    {
                //        storeIds.Add(us.StoreId);
                //    }
                //    if (storeIds != null)
                //    {
                //        foreach (var storeId in storeIds)
                //        {
                //            var orderDetails = await _repository.OrderDetailRepository.GetAllOrderDetail(null, null, storeId, null);
                //            if (orderDetails != null)
                //            {
                //                foreach (var orderDetail in orderDetails)
                //                {
                //                    if (orderDetail.OrderId != null)
                //                    {
                //                        orderIds.Add(orderDetail.OrderId);
                //                    }
                //                }
                //            }
                //        }
                //    }

                //}
                DateTime? startDateTime = startDate.HasValue ? startDate.Value.ToDateTime(TimeOnly.MinValue) : null;
                DateTime? endDateTime = endDate.HasValue ? endDate.Value.ToDateTime(TimeOnly.MaxValue) : null;
                var orders = await _repository.OrderRepository.GetAllNotPagination(null, minAmount, maxAmount, paymentMethod, status, startDateTime, endDateTime);
                return orders.Count > 0 ? orders : null;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<(List<object>?, List<object>?, int, decimal?)> GetAllStoreStaticsByDate(DateTime? date)
        {
            var stores = await _repository.StoreRepository.GetAll();
            var orderResult = new List<object>();
            var amountResult = new List<object>();
            decimal? totalProfit = 0;
            var totalOrder = 0;
            foreach (var store in stores)
            {                
                var result = await _repository.OrderRepository.GetStoreStaticsByDate(date, store.Id);
                orderResult.Add(new
                {
                    Label = store.StoreName,
                    Value = result.Item3
                });
                amountResult.Add(new
                {
                    Label = store.StoreName,
                    Value = result.Item4
                });
                totalProfit += result.Item4;
                totalOrder += result.Item3;
            }
            return (orderResult, amountResult, totalOrder, totalProfit);
        }
        public async Task<(List<object>?, List<object>?, int, decimal?)> GetAllStoreStaticsByMonth(int? month, int? year)
        {
            var stores = await _repository.StoreRepository.GetAll();
            var orderResult = new List<object>();
            var amountResult = new List<object>();
            decimal? totalProfit = 0;
            var totalOrder = 0;
            foreach (var store in stores)
            {                
                var result = await _repository.OrderRepository.GetStoreStaticsByMonth(month, year, store.Id);
                orderResult.Add(new
                {
                    Label = store.StoreName,
                    Value = result.Item3
                });
                amountResult.Add(new
                {
                    Label = store.StoreName,
                    Value = result.Item4
                });
                totalProfit += result.Item4;
                totalOrder += result.Item3;
            }
            return (orderResult, amountResult, totalOrder, totalProfit);
        }
        public async Task<(List<object>?, List<object>?, int, decimal?)> GetAllStoreStaticsByYear(int? year)
        {
            var stores = await _repository.StoreRepository.GetAll();
            var orderResult = new List<object>();
            var amountResult = new List<object>();
            decimal? totalProfit = 0;
            var totalOrder = 0;
            foreach (var store in stores)
            {               
                var result = await _repository.OrderRepository.GetStoreStaticsByYear(year, store.Id);
                orderResult.Add(new
                {
                    Label = store.StoreName,
                    Value = result.Item3
                });
                amountResult.Add(new
                {
                    Label = store.StoreName,
                    Value = result.Item4
                });
                totalProfit += result.Item4;
                totalOrder += result.Item3;
            }
            return (orderResult, amountResult, totalOrder, totalProfit);
        }
        public async Task<(List<object>?, List<object>?, int, decimal?)> GetStoreStaticsByDate(DateTime? date, Guid storeId)
        {
            var result = await _repository.OrderRepository.GetStoreStaticsByDate(date, storeId);
            return result;
        }
        public async Task<(List<object>?, List<object>?, int, decimal?)> GetStoreStaticsByMonth(int? month, int? year, Guid storeId)
        {
            var orderDetails = await _repository.OrderDetailRepository.GetAllOrderDetail(null, null, storeId, null);
            var orderIds = new List<Guid?>();
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
            var result = await _repository.OrderRepository.GetStoreStaticsByMonth(month, year, storeId);
            return result;
        }
        public async Task<(List<object>?, List<object>?, int, decimal?)> GetStoreStaticsByYear(int? year, Guid storeId)
        {
            var orderDetails = await _repository.OrderDetailRepository.GetAllOrderDetail(null, null, storeId, null);
            var orderIds = new List<Guid?>();
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
            var result = await _repository.OrderRepository.GetStoreStaticsByYear(year, storeId);
            return result;
        }
    }
}
