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
    [Route("api/streets")]
    [ApiController]
    public class StreetController(IStreetService service, IMapper mapper) : ControllerBase
    {
        private readonly IStreetService _service = service;
        private readonly IMapper _mapper = mapper;

        [Authorize]
        [HttpPost("")]
        public async Task<IActionResult> AddAStreet([FromForm]StreetModel model)
        {
            try
            {
                var result = await _service.AddAStreet(model);
                return result.Item1 == 1 ? Ok(new BaseResponse(false, "Đã tồn tại!")) :
                        result.Item1 == 2 ? Ok(new ItemResponse<StreetRequest>("Đã thêm", _mapper.Map<StreetRequest>(result))) :
                                            Ok(new BaseResponse(false, "Đã xảy ra lỗi!!!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAStreet([FromRoute] Guid id, [FromForm] StreetModel model)
        {
            try
            {
                var result = await _service.UpdateAStreet(id, model);

                return result.Item1 switch
                {
                    1 => Ok(new BaseResponse(false, "Không tồn tại!!!")),
                    2 => Ok(new ItemResponse<StreetRequest>("Đã cập nhật thông tin!", _mapper.Map<StreetRequest>(result.Item2))),
                    _ => Ok(new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại"))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> DeleteAStreet([FromRoute] Guid id)
        {
            try
            {
                var result = await _service.DeleteAStreet(id);

                return result.Item1 switch
                {
                    1 => Ok(new BaseResponse(false, "Không tồn tại!!!")),
                    2 => Ok(new ItemResponse<StreetRequest>("Đã xóa thành công!", _mapper.Map<StreetRequest>(result.Item2))),
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
        public async Task<IActionResult> GetAStreetById([FromRoute] Guid id)
        {
            try
            {
                var street = await _service.GetAStreetById(id);

                return street switch
                {
                    null => Ok(new ItemResponse<Street>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<Street>(ConstantMessage.Success, street))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [AllowAnonymous]
        [HttpGet("non-deleted")]
        public async Task<IActionResult> GetAllActiveBookCategories()
        {
            try
            {
                var streets = await _service.GetAllActiveStreets();

                return streets switch
                {
                    null => Ok(new ItemListResponse<StreetRequest>(ConstantMessage.Success, null)),
                    not null => Ok(new ItemListResponse<StreetRequest>(ConstantMessage.Success, _mapper.Map<List<StreetRequest>>(streets)))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpPost("pagination-search")]
        public async Task<IActionResult> GetAllStreetsPagination(PaginatedRequest<StreetSearchRequest> request)
        {
            try
            {
                var streets = await _service.GetAllStreetsPagination(request != null && request.Result != null ? request.Result.Key : null, request != null ? request.PageNumber : 1, request != null ? request.PageSize : 10, request != null ? request.SortField : "CreatedDate", request != null && request.SortOrder == -1);

                return streets.Item2 switch
                {
                    0 => Ok(new PaginatedListResponse<StreetRequest>(ConstantMessage.Success, null)),
                    _ => Ok(new PaginatedListResponse<StreetRequest>(ConstantMessage.Success, _mapper.Map<List<StreetRequest>>(streets.Item1), streets.Item2, request != null ? request.PageNumber : 1, request != null ? request.PageSize : 10, request != null ? request.SortField : "CreatedDate", request != null && request.SortOrder != -1 ? 1 : -1))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            };
        }
    }
}
