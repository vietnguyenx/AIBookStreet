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
        public async Task<IActionResult> AddASouvenir(SouvenirModel model)
        {
            try
            {
                var result = await _service.AddASouvenir(model);
                return result == null ? Ok(new BaseResponse(false, "Đã xảy ra lỗi!!!")) : Ok(new BaseResponse(true, "Đã thêm"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateASouvenir(SouvenirModel model)
        {
            try
            {
                var result = await _service.UpdateASouvenir(model.Id, model);

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
        public async Task<IActionResult> DeleteASouvenir([FromRoute] Guid id)
        {
            try
            {
                var result = await _service.DeleteASouvenir(id);

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
                var souvenir = await _service.GetAllSouvenirsPagination(request.Result.SouvenirName, request.PageNumber, request.PageSize, request.SortField, request.SortOrder == 0);

                return souvenir.Item2 switch
                {
                    0 => Ok(new PaginatedListResponse<SouvenirRequest>(ConstantMessage.Success, null)),
                    _ => Ok(new PaginatedListResponse<SouvenirRequest>(ConstantMessage.Success, _mapper.Map<List<SouvenirRequest>>(souvenir.Item1), souvenir.Item2, request.PageNumber, request.PageSize, request.SortField, request.SortOrder))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
    }
}
