using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.API.SearchModel;
using AIBookStreet.API.Tool.Constant;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AIBookStreet.Services.Services.Service;
using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using System.Diagnostics;

namespace AIBookStreet.API.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController(IOrderService service, IMapper mapper, IPayOSService payOSService) : ControllerBase
    {
        private readonly IOrderService _service = service;
        private readonly IPayOSService _payOSService = payOSService;
        private readonly IMapper _mapper = mapper;

        [Authorize]
        [HttpPost("")]
        public async Task<IActionResult> AddAnOrder([FromForm] OrderModel model)
        {
            try
            {
                var result = await _service.AddAnOrder(model);
                if (result.Item1 == 2)
                {
                    var payoss = await _payOSService.CreatePaymentLink(result.Item2.Id);
                    if (payoss.Item1 == 4)
                    {
                        var res = _mapper.Map<OrderRequest>(result.Item2);
                        res.PaymentLink = payoss.Item2?.checkoutUrl;
                        return Ok(new ItemResponse<OrderRequest>("Đã thêm đơn hàng", res));
                    }
                    return payoss.Item1 switch
                    {
                        0 => Ok(new BaseResponse(false, "ClientId not found")),
                        1 => Ok(new BaseResponse(false, "ApiKey not found")),
                        2 => Ok(new BaseResponse(false, "ChecksumKey not found")),
                        _ => Ok(new BaseResponse(false, "Order not found!!"))
                    };
                }
                return result.Item1 switch
                {
                    1 => BadRequest(new BaseResponse(false, result.Item3)),
                    _ => BadRequest(new BaseResponse(false, result.Item3))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPatch("confirm-order/{orderId}")]
        public async Task<IActionResult> Confirm([FromRoute] Guid orderId)
        {
            try
            {
                var result = await _service.ConfirmOrder(orderId);
                return result.Item1 switch
                {
                    1 => BadRequest(new BaseResponse(false, result.Item3)),
                    _ => Ok(new ItemResponse<OrderRequest>("Đã xác nhận đơn hàng", _mapper.Map<OrderRequest>(result.Item2)))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPatch("cancel-order/{orderId}")]
        public async Task<IActionResult> Cancel([FromRoute] Guid orderId)
        {
            try
            {
                var result = await _service.CancelAnOrder(orderId);
                return result.Item1 switch
                {
                    1 => BadRequest(new BaseResponse(false, result.Item3)),
                    _ => Ok(new ItemResponse<OrderRequest>("Đã hủy đơn hàng", _mapper.Map<OrderRequest>(result.Item2)))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAnOrderById([FromRoute] Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("Id is empty");
                }
                var order = await _service.GetAnOrderById(id);
                if (order == null){
                    return BadRequest(new ItemResponse<Order>(ConstantMessage.NotFound));
                }
                var orderItems = new List<object>();
                foreach (var item in order.OrderDetails)
                {
                    orderItems.Add(new
                    {
                        id = item.Id,
                        productName = item.Inventory.Book != null ? item.Inventory.Book.Title : item.Inventory.Souvenir.SouvenirName,
                        imgUrl = item.Inventory.Book != null ? item.Inventory.Book.Images.First().Url : item.Inventory.Souvenir.BaseImgUrl,
                        quantity = item.Quantity,
                        price =(int?) (item.Inventory.Book != null ? item.Inventory.Book.Price : item.Inventory.Souvenir.Price)
                    });
                }

                var response = new
                {
                    id = order.Id,
                    totalAmount = order.TotalAmount,
                    paymentMethod = order.PaymentMethod,
                    status = order.Status,
                    store = order.OrderDetails.First().Inventory.Store.StoreName,
                    createDate = order.CreatedDate,
                    orderDetails = orderItems
                };
                return Ok(new ItemResponse<object>(ConstantMessage.Success, response));
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [AllowAnonymous]
        [HttpPost("search/paginated")]
        public async Task<IActionResult> GetAllOrderPagination(PaginatedRequest<OrderSearchRequest>? request)
        {
            try
            {
                var orders = request != null ? await _service.GetPaginationOrders(request.Result?.MinAmount, request.Result?.MaxAmount, request.Result?.PaymentMethod, request.Result?.Status, request.Result?.StartDate, request.Result?.EndDate, request.Result?.StoreId, request.PageNumber, request.PageSize, request.SortField, request.SortOrder)
                                             : await _service.GetPaginationOrders(null,null, null, null, null, null, null, 1,10, "CreatedDate", -1);

                return orders.Item2 switch
                {
                    0 => Ok(new PaginatedListResponse<OrderRequest>(ConstantMessage.Success, null)),
                    -1 => BadRequest(new BaseResponse(false, "Vui lòng đăng nhập vào cửa hàng!!!")),
                    _ => Ok(new PaginatedListResponse<OrderRequest>(ConstantMessage.Success, _mapper.Map<List<OrderRequest>>(orders.Item1), orders.Item2, request != null ? request.PageNumber : 1, request != null ? request.PageSize : 10, request != null ? request.SortField : "CreatedDate", request != null && request.SortOrder != -1 ? -1 : 1))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [AllowAnonymous]
        [HttpPost("search")]
        public async Task<IActionResult> GetAllOrder(OrderSearchRequest? request)
        {
            try
            {
                var orders = request != null ? await _service.GetAllOrders(request?.MinAmount, request?.MaxAmount, request?.PaymentMethod, request?.Status, request?.StartDate, request?.EndDate, request?.StoreId)
                                             : await _service.GetAllOrders(null, null, null, null, null, null, null);

                return orders switch
                {
                    null => BadRequest(new BaseResponse(false, "Vui lòng đăng nhập vào cửa hàng")),
                    not null => Ok(new ItemListResponse<OrderRequest>(ConstantMessage.Success, _mapper.Map<List<OrderRequest>>(orders)))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [AllowAnonymous]
        [HttpGet("statics-for-admin/daily")]
        public async Task<IActionResult> GetAllStoreStaticsByDate([FromQuery]DateTime? date)
        {
            try
            {
                var orders = await _service.GetAllStoreStaticsByDate(date);
                return Ok(new
                {
                    success = true,
                    orderChart = orders.Item1,
                    orderProfit = orders.Item2,
                    totalOrder = orders.Item3,
                    totalProfit = orders.Item4
                });
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [AllowAnonymous]
        [HttpGet("statics-for-admin/monthly/{month}/{year}")]
        public async Task<IActionResult> GetAllStoreStaticsByMonth([FromRoute] int? month, int? year)
        {
            try
            {
                var orders = await _service.GetAllStoreStaticsByMonth(month, year);
                return Ok(new
                {
                    success = true,
                    orderChart = orders.Item1,
                    orderProfit = orders.Item2,
                    totalOrder = orders.Item3,
                    totalProfit = orders.Item4
                });
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [AllowAnonymous]
        [HttpGet("statics-for-admin/yearly/{year}")]
        public async Task<IActionResult> GetAllStoreStaticsByYear([FromRoute]int? year)
        {
            try
            {
                var orders = await _service.GetAllStoreStaticsByYear(year);
                return Ok(new
                {
                    success = true,
                    orderChart = orders.Item1,
                    orderProfit = orders.Item2,
                    totalOrder = orders.Item3,
                    totalProfit = orders.Item4
                });
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [AllowAnonymous]
        [HttpGet("statics-for-store/daily")]
        public async Task<IActionResult> GetStoreStaticsByDate([FromQuery] DateTime? date, Guid storeId)
        {
            try
            {
                var orders = await _service.GetStoreStaticsByDate(date, storeId);
                return Ok(new
                {
                    success = true,
                    orderChart = orders.Item1,
                    orderProfit = orders.Item2,
                    totalOrder = orders.Item3,
                    totalProfit = orders.Item4
                });
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [AllowAnonymous]
        [HttpGet("statics-for-store/monthly/{month}/{year}/{storeId}")]
        public async Task<IActionResult> GetStoreStaticsByMonth([FromRoute] int? month, int? year, Guid storeId)
        {
            try
            {
                var orders = await _service.GetStoreStaticsByMonth(month, year, storeId);
                return Ok(new
                {
                    success = true,
                    orderChart = orders.Item1,
                    orderProfit = orders.Item2,
                    totalOrder = orders.Item3,
                    totalProfit = orders.Item4
                });
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [AllowAnonymous]
        [HttpGet("statics-for-store/yearly/{year}/{storeId}")]
        public async Task<IActionResult> GetStoreStaticsByYear([FromRoute] int? year, Guid storeId)
        {
            try
            {
                var orders = await _service.GetStoreStaticsByYear(year, storeId);
                return Ok(new
                {
                    success = true,
                    orderChart = orders.Item1,
                    orderProfit = orders.Item2,
                    totalOrder = orders.Item3,
                    totalProfit = orders.Item4
                });
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
    }
}
