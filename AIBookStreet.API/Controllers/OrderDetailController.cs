﻿using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.API.SearchModel;
using AIBookStreet.API.Tool.Constant;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AIBookStreet.API.Controllers
{
    [Route("api/order-details")]
    [ApiController]
    public class OrderDetailController(IOrderDetailService service, IMapper mapper) : ControllerBase
    {
        private readonly IOrderDetailService _service = service;
        private readonly IMapper _mapper = mapper;
        [Authorize]
        [HttpPost("")]
        public async Task<IActionResult> AddToCart([FromForm] OrderDetailModel model)
        {
            try
            {
                var result = await _service.AddToCart(model);
                return result == null ? Ok(new BaseResponse(false, "Đã xảy ra lỗi!!!")) : Ok(new ItemResponse<OrderDetailRequest>("Đã thêm vào giỏ", _mapper.Map<OrderDetailRequest>(result)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnOrderDetail([FromRoute] Guid id)
        {
            try
            {
                var result = await _service.RemoveFromCart(id);

                return result.Item1 switch
                {
                    1 => Ok(new BaseResponse(false, "Không tồn tại!!!")),
                    2 => Ok(new ItemResponse<OrderDetailRequest>("Đã xóa thành công!", _mapper.Map<OrderDetailRequest>(result.Item2))),
                    _ => Ok(new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại!!!"))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAnOrderDetailById([FromRoute] Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("Id is empty");
                }
                var orderDetail = await _service.GetByID(id);

                return orderDetail.Item1 switch
                {
                    0 => Ok(new ItemResponse<OrderDetail>(ConstantMessage.NotFound)),
                    _ => Ok(new ItemResponse<OrderDetail>(ConstantMessage.Success, orderDetail.Item2))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [Authorize]
        [HttpGet("cart/{storeId}")]
        public async Task<IActionResult> GetCartByStoreId([FromRoute] Guid storeId)
        {
            try
            {
                if (storeId == Guid.Empty)
                {
                    return BadRequest("Id is empty");
                }
                var cart = await _service.GetCart(storeId);

                return cart.Item1 switch
                {
                    0 => Ok(new ItemResponse<OrderDetail>(ConstantMessage.NotFound)),
                    _ => Ok(new ItemListResponse<OrderDetail>(ConstantMessage.Success, cart.Item2))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [AllowAnonymous]
        [HttpPost("search")]
        public async Task<IActionResult> GetAllOrderDetail(OrderDetailSearchRequest? request)
        {
            try
            {
                var orderDetails = request != null ? await _service.GetAllOrderDetail(request?.OrderId, request?.StoreId, request?.EntityId)
                                             : await _service.GetAllOrderDetail(null, null, null);

                return orderDetails switch
                {
                    null => Ok(new ItemListResponse<OrderDetailRequest>(ConstantMessage.Success, null)),
                    not null => Ok(new ItemListResponse<OrderDetailRequest>(ConstantMessage.Success, _mapper.Map<List<OrderDetailRequest>>(orderDetails)))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
    }
}
