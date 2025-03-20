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
    public class SouvenirController(ISouvenirService service, IMapper mapper) : ControllerBase
    {
        private readonly ISouvenirService _service = service;
        private readonly IMapper _mapper = mapper;

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddASouvenir([FromForm]SouvenirModel model)
        {
            try
            {
                var result = await _service.AddASouvenir(model);
                return result == null ? Ok(new BaseResponse(false, "Đã xảy ra lỗi!!!")) : Ok(new ItemResponse<SouvenirRequest>("Đã thêm", _mapper.Map<SouvenirRequest>(result)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateASouvenir([FromRoute] Guid id, [FromForm]SouvenirModel model)
        {
            try
            {
                var result = await _service.UpdateASouvenir(id, model);

                return result.Item1 switch
                {
                    1 => Ok(new BaseResponse(false, "Không tồn tại!!!")),
                    2 => Ok(new ItemResponse<SouvenirRequest>("Đã cập nhật thông tin!", _mapper.Map<SouvenirRequest>(result.Item2))),
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
        public async Task<IActionResult> DeleteASouvenir([FromRoute] Guid id)
        {
            try
            {
                var result = await _service.DeleteASouvenir(id);

                return result.Item1 switch
                {
                    1 => Ok(new BaseResponse(false, "Không tồn tại!!!")),
                    2 => Ok(new ItemResponse<SouvenirRequest>("Đã xóa thành công!", _mapper.Map<SouvenirRequest>(result.Item2))),
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
        public async Task<IActionResult> GetASouvenirById([FromRoute] Guid id)
        {
            try
            {
                var souvenir = await _service.GetASouvenirById(id);

                return souvenir switch
                {
                    null => Ok(new ItemResponse<Souvenir>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<Souvenir>(ConstantMessage.Success, souvenir))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [AllowAnonymous]
        [HttpPost("pagination-and-search")]
        public async Task<IActionResult> GetAllSouvenirsPagination(PaginatedRequest<SouvenirSearchRequest> request)
        {
            try
            {
                var souvenir = await _service.GetAllSouvenirsPagination(request != null && request.Result != null ? request.Result.SouvenirName : null, request != null ? request.PageNumber : 1, request != null ? request.PageSize : 10, request != null ? request.SortField : "CreatedDate", request != null && request.SortOrder == -1);

                return souvenir.Item2 switch
                {
                    0 => Ok(new PaginatedListResponse<SouvenirRequest>(ConstantMessage.Success, null)),
                    _ => Ok(new PaginatedListResponse<SouvenirRequest>(ConstantMessage.Success, _mapper.Map<List<SouvenirRequest>>(souvenir.Item1), souvenir.Item2, request != null ? request.PageNumber : 1, request != null ? request.PageSize : 10, request != null ? request.SortField : "CreatedDate", request != null && request.SortOrder != -1 ? 1 : -1))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
    }
}
