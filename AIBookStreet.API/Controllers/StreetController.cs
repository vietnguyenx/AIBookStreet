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
    [Route("api/[controller]")]
    [ApiController]
    public class StreetController(IStreetService service, IMapper mapper) : ControllerBase
    {
        private readonly IStreetService _service = service;
        private readonly IMapper _mapper = mapper;

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddAStreet(StreetModel model)
        {
            try
            {
                var result = await _service.AddAStreet(model);
                return result.Item1 == 1 ? Ok(new BaseResponse(false, "Đã tồn tại!")) :
                        result.Item1 == 2 ? Ok(new BaseResponse(true, "Đã thêm")) :
                                            Ok(new BaseResponse(false, "Đã xảy ra lỗi!!!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAStreet(StreetModel model)
        {
            try
            {
                var result = await _service.UpdateAStreet(model.Id, model);

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
        [Authorize]
        [HttpPut("delete/{id}")]
        public async Task<IActionResult> DeleteAStreet([FromRoute] Guid id)
        {
            try
            {
                var result = await _service.DeleteAStreet(id);

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
        [AllowAnonymous]
        [HttpGet("get-by-id/{id}")]
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
        [HttpGet("get-all-active")]
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
        [HttpPost("pagination-and-search")]
        public async Task<IActionResult> GetAllStreetsPagination(PaginatedRequest<StreetSearchRequest> request)
        {
            try
            {
                var streets = await _service.GetAllStreetsPagination(request.Result.Key, request.PageNumber, request.PageSize, request.SortField, request.SortOrder == 0);

                return streets.Item2 switch
                {
                    0 => Ok(new PaginatedListResponse<StreetRequest>(ConstantMessage.Success, null)),
                    _ => Ok(new PaginatedListResponse<StreetRequest>(ConstantMessage.Success, _mapper.Map<List<StreetRequest>>(streets.Item1), streets.Item2, request.PageNumber, request.PageSize, request.SortField, request.SortOrder))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            };
        }
    }
}
