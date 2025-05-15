using AIBookStreet.API.RequestModel;
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
    [Route("api/zones")]
    [ApiController]
    public class ZoneController(IZoneService service, IMapper mapper) : ControllerBase
    {
        private readonly IZoneService _service = service;
        private readonly IMapper _mapper = mapper;

        [Authorize]
        [HttpPost("")]
        public async Task<IActionResult> AddAZone(ZoneModel model)
        {
            try
            {
                var result = await _service.AddAZone(model);
                return result == null ? BadRequest(new BaseResponse(false, "Đã xảy ra lỗi!!!")) : Ok(new BaseResponse(true, "Đã thêm"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAZone([FromRoute] Guid id, ZoneModel model)
        {
            try
            {
                var result = await _service.UpdateAZone(id, model);

                return result.Item1 switch
                {
                    1 => BadRequest(new BaseResponse(false, "Không tồn tại!!!")),
                    2 => Ok(new BaseResponse(true, "Đã cập nhật thông tin!")),
                    _ => BadRequest(new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại"))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> DeleteAZone([FromRoute] Guid id)
        {
            try
            {
                var result = await _service.DeleteAZone(id);

                return result.Item1 switch
                {
                    1 => BadRequest(new BaseResponse(false, "Không tồn tại!!!")),
                    2 => Ok(new BaseResponse(true, "Đã xóa thông tin!")),
                    _ => BadRequest(new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại"))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAZoneById([FromRoute] Guid id)
        {
            try
            {
                var zone = await _service.GetAZoneById(id);

                return zone switch
                {
                    null => BadRequest(new ItemResponse<Zone>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<Zone>(ConstantMessage.Success, zone))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [AllowAnonymous]
        [HttpGet("non-deleted")]
        public async Task<IActionResult> GetAllActiveZones()
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
        [AllowAnonymous]
        [HttpPost("search/paginated")]
        public async Task<IActionResult> GetAllZonesPagination(PaginatedRequest<ZoneSearchRequest> request)
        {
            try
            {
                var zones = await _service.GetAllZonesPagination(request != null && request.Result != null ? request.Result.ZoneName : null, request != null && request.Result != null ? request.Result.StreetId : null, request != null ? request.PageNumber : 1, request != null ? request.PageSize : 10, request != null ? request.SortField : "CreatedDate", request != null && request.SortOrder == -1);

                return zones.Item2 switch
                {
                    0 => Ok(new PaginatedListResponse<ZoneRequest>(ConstantMessage.Success, null)),
                    _ => Ok(new PaginatedListResponse<ZoneRequest>(ConstantMessage.Success, _mapper.Map<List<ZoneRequest>>(zones.Item1), zones.Item2, request != null ? request.PageNumber : 1, request != null ? request.PageSize : 10, request != null ? request.SortField : "CreatedDate", request != null && request.SortOrder != -1 ? 1 : -1))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [AllowAnonymous]
        [HttpGet("street/{streetID}")]
        public async Task<IActionResult> GetAllByStreetID([FromRoute]Guid streetID)
        {
            try
            {
                var zones = await _service.GetAllByStreetId(streetID);

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
    }
}
