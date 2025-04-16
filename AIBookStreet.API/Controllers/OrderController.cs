using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.API.SearchModel;
using AIBookStreet.API.Tool.Constant;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIBookStreet.API.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController(IOrderService service, IMapper mapper) : ControllerBase
    {
        private readonly IOrderService _service = service;
        private readonly IMapper _mapper = mapper;

        [Authorize]
        [HttpPost("")]
        public async Task<IActionResult> AddAnOrder([FromForm] OrderModel model)
        {
            try
            {
                var result = await _service.AddAnOrder(model);
                return result.Item1 switch
                {
                    1 => Ok(new BaseResponse(false, "Số lượng hàng trong kho không còn đủ!!")),
                    2 => Ok(new ItemResponse<OrderRequest>("Đã thêm đơn hàng", _mapper.Map<OrderRequest>(result.Item2))),
                    _ => Ok(new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại"))
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

                return order switch
                {
                    null => Ok(new ItemResponse<Order>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<Order>(ConstantMessage.Success, order))
                };
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
                var orders = request != null ? await _service.GetPaginationOrders(request.Result?.MinAmount, request.Result?.MaxAmount, request.Result?.PaymentMethod, request.Result?.Status, request.Result?.StartDate, request.Result?.EndDate, request.PageNumber, request.PageSize, request.SortField, request.SortOrder)
                                             : await _service.GetPaginationOrders(null,null, null, null, null, null, 1,10, "CreatedDate", -1);

                return orders.Item2 switch
                {
                    0 => Ok(new PaginatedListResponse<OrderRequest>(ConstantMessage.Success, null)),
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
                var orders = request != null ? await _service.GetAllOrders(request?.MinAmount, request?.MaxAmount, request?.PaymentMethod, request?.Status, request?.StartDate, request?.EndDate)
                                             : await _service.GetAllOrders(null, null, null, null, null, null);

                return orders switch
                {
                    null => Ok(new ItemListResponse<OrderRequest>(ConstantMessage.Success, null)),
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
