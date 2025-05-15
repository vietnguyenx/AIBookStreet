using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.UnitOfWork.Interface;
using AIBookStreet.Services.Base;
using AIBookStreet.Services.Common;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using Google.Apis.Storage.v1.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AIBookStreet.Services.Common.ConstantMessage;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AIBookStreet.Services.Services.Service
{
    public class OrderService(IUnitOfWork repository, IMapper mapper, IHttpContextAccessor httpContextAccessor, IConfiguration configuration) : BaseService<Order>(mapper, repository, httpContextAccessor), IOrderService
    {
        private readonly IUnitOfWork _repository = repository;
        private readonly IConfiguration _configuration = configuration;
        public async Task<(long, Order?, string?)> AddAnOrder(OrderModel model)
        {
            try
            {                
                var orderDetails = await _repository.OrderDetailRepository.GetCartForCreateOrder(model.StoreId);
                decimal? totalAmount = 0;
                if (model.PaymentMethod != PaymentMethodConstant.CASH && model.PaymentMethod != PaymentMethodConstant.TRANSFER)
                {
                    return (1, null, "Phương thức thanh toán không hợp lệ");
                }
                var store = await _repository.StoreRepository.GetById(model.StoreId);
                if (store == null)
                {
                    return (1, null, "Không tìm thấy thông tin cửa hàng");
                }
                var order = new Order
                {
                    PaymentMethod = model.PaymentMethod,
                    StoreId = model.StoreId
                };
                var setOrder = await SetBaseEntityToCreateFunc(order);
                List<ItemData> items = [];
                if (orderDetails != null && orderDetails.Count > 0)
                {
                    foreach (var orderDetail in orderDetails)
                    {
                        var inventory = await _repository.InventoryRepository.GetByID(orderDetail.InventoryId);
                        if (inventory == null)
                        {
                            return (1, null, "Không tìm thấy thông tin kho hàng");
                        }

                        if (inventory != null && inventory.Book != null)
                        {
                            if (inventory.Quantity < orderDetail.Quantity)
                            {
                                return (1, null, "Số lượng của '" + inventory.Book.Title + "' không đủ"); //sl trong inventory khong du
                            }
                            else
                            {
                                var price = (int)inventory.Book.Price * orderDetail.Quantity;
                                items.Add(new ItemData(inventory.Book.Title, orderDetail.Quantity, price));

                                inventory.Quantity -= orderDetail.Quantity;
                                inventory.IsInStock = inventory.Quantity > 0;
                                inventory.LastUpdatedDate = DateTime.Now;

                                totalAmount += inventory.Book.Price * orderDetail.Quantity;
                            }
                        }
                        if (inventory != null && inventory.Souvenir != null)
                        {
                            if (inventory.Quantity < orderDetail.Quantity)
                            {
                                return (1, null, "Số lượng của '" + inventory.Souvenir.SouvenirName + "' không đủ"); //sl trong inventory khong du
                            }
                            else
                            {
                                var price = (int)inventory.Souvenir.Price * orderDetail.Quantity;
                                items.Add(new ItemData(inventory.Souvenir.SouvenirName, orderDetail.Quantity, price));

                                inventory.Quantity -= orderDetail.Quantity;
                                inventory.IsInStock = inventory.Quantity > 0;
                                inventory.LastUpdatedDate = DateTime.Now;

                                totalAmount += inventory.Souvenir.Price * orderDetail.Quantity;

                                
                            }
                        }
                        var result = await _repository.InventoryRepository.Update(inventory);
                        if (!result)
                        {
                            return (3, null, "Không thể cập nhật số lượng trong kho");
                        }
                    }
                    setOrder.TotalAmount = totalAmount;
                    setOrder.Status = OrderConstant.ORDER_PROGRESS;
                    var isSuccess = await _repository.OrderRepository.Add(setOrder);
                    var orderId = setOrder.Id;
                    if (isSuccess)
                    {
                        foreach (var orderDetail in orderDetails)
                        {
                            var item = await _repository.OrderDetailRepository.GetForCreateOrder(orderDetail.Id);
                            if (item == null)
                            {
                                return (1, null, "Không tìm thấy thông tin đơn hàng");
                            }
                            item.OrderId = orderId;
                            item.LastUpdatedDate = DateTime.UtcNow;
                            var result = await _repository.OrderDetailRepository.Update(item);
                            if (!result)
                            {
                                await _repository.OrderRepository.Remove(setOrder);
                                return (3, null, "Không thể cập nhật chi tiết đơn hàng");
                            }
                        }
                        if (model.PaymentMethod == PaymentMethodConstant.CASH)
                        {
                            return (2, setOrder, null);
                        } else if (model.PaymentMethod.Equals(PaymentMethodConstant.TRANSFER))
                        {
                            var clientId = _configuration["payOS:ClientId"];
                            if (clientId == null)
                            {
                                await _repository.OrderRepository.Remove(setOrder);
                                return (0, null, "ClientId not found"); //ClientId not found
                            }
                            var apiKey = _configuration["payOS:ApiKey"];
                            if (apiKey == null)
                            {
                                await _repository.OrderRepository.Remove(setOrder);
                                return (0, null, "ApiKey not found"); //ApiKey not found
                            }
                            var checksumKey = _configuration["payOS:ChecksumKey"];
                            if (checksumKey == null)
                            {
                                await _repository.OrderRepository.Remove(setOrder);
                                return (0, null, "ChecksumKey not found"); //ChecksumKey not found
                            }
                            PayOS _payOS = new(
                                clientId,
                                apiKey,
                                checksumKey
                            );
                            int orderCode = await _repository.OrderRepository.GetNumberOrders();
                            long expiredAt = (long)(DateTime.UtcNow.AddMinutes(10) - new DateTime(1970, 1, 1)).TotalSeconds;

                            PaymentData paymentData = new(
                                orderCode: orderCode,
                                amount: (int)totalAmount,
                                description: "Thanh toan hoa don #"+orderCode,
                                items: items,
                                cancelUrl: "https://smart-book-street-next-aso3.vercel.app/orders",
                                returnUrl: "https://smart-book-street-next-aso3.vercel.app/orders",
                                expiredAt: expiredAt
                            );

                            CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);
                            return (2, setOrder, createPayment.checkoutUrl);
                        }
                    }
                }
                return (3, null, "Đơn hàng chưa có vật phẩm");
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<(long, Order?, string?)> ConfirmOrder(Guid id)
        {
            try
            {
                var existed = await _repository.OrderRepository.GetByIdForUpdateStatus(id);
                if (existed == null)
                {
                    return (1, null, "Đơn hàng không tồn tại"); //khong ton tai
                }
                if (existed.Status == OrderConstant.ORDER_CANCELLED || existed.Status == OrderConstant.ORDER_COMPLETED)
                {
                    return (1, null, "Không thể xác nhận đơn đã hủy/thanh toán");
                }
                existed.Status = OrderConstant.ORDER_COMPLETED;
                existed = await SetBaseEntityToUpdateFunc(existed);

                var updateSuccess = await _repository.OrderRepository.Update(existed);
                if (updateSuccess)
                {                    
                    return (2, existed, "Đã xác nhận đơn hàng"); //update thành công
                }
                return (1, null, "Đã xảy ra lỗi, vui lòng kiểm tra lại");
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<(long, Order?, string?)> CancelAnOrder(Guid id)
        {
            try
            {
                var existed = await _repository.OrderRepository.GetByIdForUpdateStatus(id);
                if (existed == null)
                {
                    return (1, null, "Đơn hàng không tồn tại"); //khong ton tai
                }
                if (existed.Status == OrderConstant.ORDER_CANCELLED || existed.Status == OrderConstant.ORDER_COMPLETED)
                {
                    return (1, null, "Không thể hủy đơn đã hủy/thanh toán");
                }
                existed.Status = OrderConstant.ORDER_CANCELLED;
                existed = await SetBaseEntityToUpdateFunc(existed);

                var updateSuccess = await _repository.OrderRepository.Update(existed);
                if (updateSuccess)
                {
                    return (2, existed, "Đã hủy đơn hàng"); //update thành công
                }
                return (1, null, "Đã xảy ra lỗi, vui lòng kiểm tra lại");
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Order?> GetAnOrderById(Guid orderId)
        {
            try
            {
                return await _repository.OrderRepository.GetByID(orderId);
            } catch
            {
                throw;
            }
        }
        public async Task<(List<Order>?, long, string?)> GetPaginationOrders(decimal? minAmount, decimal? maxAmount, string? paymentMethod, string? status, DateTime? startDate, DateTime? endDate, Guid? storeId, int? pageNumber, int? pageSize, string? sortField, int? sortOrder)
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
                if (storeId == null)
                {
                    return (null, -1, null);
                }
                var orders = await _repository.OrderRepository.GetAllPagination(null, minAmount, maxAmount, paymentMethod, status, startDate, endDate, storeId, pageNumber, pageSize, sortField, sortOrder);
                var clientId = _configuration["payOS:ClientId"];
                if (clientId == null)
                {
                    return (null, 0, "ClientId not found");
                }
                var apiKey = _configuration["payOS:ApiKey"];
                if (apiKey == null)
                {
                    return (null, 0, "ApiKey not found"); //ApiKey not found
                }
                var checksumKey = _configuration["payOS:ChecksumKey"];
                if (checksumKey == null)
                {
                    return (null, 0, "ChecksumKey not found"); //ChecksumKey not found
                }
                PayOS _payOS = new(clientId, apiKey, checksumKey);
                if (orders.Item1 != null && orders.Item1.Count > 0)
                {
                    var index = await _repository.OrderRepository.GetAll();
                        var item = orders.Item1.FirstOrDefault();
                        if (item != null && item.PaymentMethod == "Transfer")
                        {
                            PaymentLinkInformation paymentLinkInformation = await _payOS.getPaymentLinkInformation(index.Count);
                            if (paymentLinkInformation.status == "CANCELLED" && item.Status == "InProgress")
                            {
                                item.Status = OrderConstant.ORDER_CANCELLED;
                                var updateSuccess = await _unitOfWork.OrderRepository.Update(item);
                                if (!updateSuccess)
                                {
                                    return (null, 0, "Lỗi trạng thái đơn hàng"); //update fail
                                }
                            }
                        }
                                    
                }
                return orders.Item1?.Count > 0 ? (orders.Item1, orders.Item2, null) : (null, 1, null);
            } catch (Exception)
            {
                throw;
            }
        }
        public async Task<(List<Order>?, string?)> GetAllOrders(decimal? minAmount, decimal? maxAmount, string? paymentMethod, string? status, DateTime? startDate, DateTime? endDate, Guid? storeId)
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
                if (storeId == null)
                {
                    return (null, "Không tìm thấy thông tin cửa hàng");
                }
                var orders = await _repository.OrderRepository.GetAllNotPagination(null, minAmount, maxAmount, paymentMethod, status, startDate, endDate, storeId);
                var clientId = _configuration["payOS:ClientId"];
                if (clientId == null)
                {
                    return (null, "ClientId not found");
                }
                var apiKey = _configuration["payOS:ApiKey"];
                if (apiKey == null)
                {
                    return (null, "ApiKey not found"); //ApiKey not found
                }
                var checksumKey = _configuration["payOS:ChecksumKey"];
                if (checksumKey == null)
                {
                    return (null, "ChecksumKey not found"); //ChecksumKey not found
                }
                PayOS _payOS = new(clientId, apiKey, checksumKey);
                if (orders != null && orders.Count > 0)
                {
                    var index = await _repository.OrderRepository.GetAll();
                    var item = orders.LastOrDefault();
                    if (item != null && item.PaymentMethod == "Transfer")
                    {
                        PaymentLinkInformation paymentLinkInformation = await _payOS.getPaymentLinkInformation(index.Count);
                        if (paymentLinkInformation.status == "CANCELLED" && item.Status == "InProgress")
                        {
                            item.Status = OrderConstant.ORDER_CANCELLED;
                            var updateSuccess = await _unitOfWork.OrderRepository.Update(item);
                            if (!updateSuccess)
                            {
                                return (null, "Lỗi trạng thái đơn hàng"); //update fail
                            }
                        }
                    }

                }
                return orders?.Count > 0 ? (orders, null) : (null, "Vui lòng đăng nhập vào cửa hàng");
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<(List<object>?, List<object>?, int, decimal?)> GetAllStoreStaticsByDate(DateTime? date)
        {
            try
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
            } catch
            {
                throw;
            }
        }
        public async Task<(List<object>?, List<object>?, int, decimal?)> GetAllStoreStaticsByMonth(int? month, int? year)
        {
            try
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
            } catch
            {
                throw;
            }
        }
        public async Task<(List<object>?, List<object>?, int, decimal?)> GetAllStoreStaticsByYear(int? year)
        {
            try
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
            } catch
            {
                throw;
            }
        }
        public async Task<(List<object>?, List<object>?, int, decimal?)> GetStoreStaticsByDate(DateTime? date, Guid storeId)
        {
            try
            {
                var result = await _repository.OrderRepository.GetStoreStaticsByDate(date, storeId);
                return result;
            } catch
            {
                throw;
            }
        }
        public async Task<(List<object>?, List<object>?, int, decimal?)> GetStoreStaticsByMonth(int? month, int? year, Guid storeId)
        {
            try
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
            } catch
            {
                throw;
            }
        }
        public async Task<(List<object>?, List<object>?, int, decimal?)> GetStoreStaticsByYear(int? year, Guid storeId)
        {
            try
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
            } catch
            {
                throw;
            }
        }
    }
}
