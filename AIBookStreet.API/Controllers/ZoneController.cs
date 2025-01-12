﻿using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.API.Tool.Constant;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AIBookStreet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZoneController(IZoneService service, IMapper mapper) : ControllerBase
    {
        private readonly IZoneService _service = service;
        private readonly IMapper _mapper = mapper;

        //[Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddAZone([FromQuery] ZoneModel model)
        {
            try
            {
                var result = await _service.AddAZone(model);
                return result == null ? Ok(new BaseResponse(false, "Đã xảy ra lỗi!!!")) : Ok(new BaseResponse(true, "Đã thêm"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateAZone([FromRoute] Guid id, [FromQuery] ZoneModel model)
        {
            try
            {
                var result = await _service.UpdateAZone(id, model);

                return result.Item1 switch
                {
                    1 => Ok(new BaseResponse(false, "Không tồn tại!!!")),
                    2 => Ok(new BaseResponse(true, "Đã cập nhật thông tin!")),
                    _ => Ok(new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại"))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[Authorize]
        [HttpPut("delete/{id}")]
        public async Task<IActionResult> DeleteAZone([FromRoute] Guid id)
        {
            try
            {
                var result = await _service.DeleteAZone(id);

                return result.Item1 switch
                {
                    1 => Ok(new BaseResponse(false, "Không tồn tại!!!")),
                    2 => Ok(new BaseResponse(true, "Đã xóa thông tin!")),
                    _ => Ok(new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại"))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetAZoneById([FromRoute] Guid id)
        {
            try
            {
                var zone = await _service.GetAZoneById(id);

                return zone switch
                {
                    null => Ok(new ItemResponse<Zone>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<Zone>(ConstantMessage.Success, zone))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [HttpGet("get-all-active")]
        public async Task<IActionResult> GetAllActiveCategories()
        {
            try
            {
                var zones = await _service.GetAllActiveZones();

                return zones switch
                {
                    null => Ok(new ItemListResponse<ZoneRequest>(ConstantMessage.Success, null)),
                    not null => Ok(new ItemListResponse<ZoneRequest>(ConstantMessage.Success, _mapper.Map<List<ZoneRequest>>(zones)))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("pagination-and-search")]
        public async Task<IActionResult> GetAllZonesPagination(string? key, Guid? streetID, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            try
            {
                var zones = await _service.GetAllZonesPagination(key, streetID, pageNumber, pageSize, sortField, desc);

                return zones.Item2 switch
                {
                    0 => Ok(new PaginatedListResponse<ZoneRequest>(ConstantMessage.Success, null)),
                    _ => Ok(new PaginatedListResponse<ZoneRequest>(ConstantMessage.Success, _mapper.Map<List<ZoneRequest>>(zones.Item1), zones.Item2, pageNumber != null ? (int)pageNumber : 1, pageSize != null ? (int)pageSize : 10, sortField, desc != null && (desc != false) ? 0 : 1))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
    }
}
