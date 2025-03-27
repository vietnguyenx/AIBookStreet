using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.API.SearchModel;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Common;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AIBookStreet.API.Controllers
{
    [Route("api/souvenirs")]
    [ApiController]
    public class SouvenirController(ISouvenirService service, IMapper mapper) : ControllerBase
    {
        private readonly ISouvenirService _service = service;
        private readonly IMapper _mapper = mapper;

        [Authorize]
        [HttpPost("")]
        public async Task<IActionResult> AddASouvenir([FromForm]SouvenirModel model)
        {
            try
            {
                var result = await _service.AddASouvenir(model);
                return result == null ? StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, "Đã xảy ra lỗi")) : StatusCode(ConstantHttpStatus.CREATED, new ItemResponse<SouvenirRequest>("Đã thêm", _mapper.Map<SouvenirRequest>(result)));
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            }
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateASouvenir([FromRoute] Guid id, [FromForm]SouvenirModel model)
        {
            try
            {
                var result = await _service.UpdateASouvenir(id, model);

                return result.Item1 switch
                {
                    1 => StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, "Không tồn tại!!!")),
                    2 => StatusCode(ConstantHttpStatus.OK,new ItemResponse<SouvenirRequest>("Đã cập nhật thông tin!", _mapper.Map<SouvenirRequest>(result.Item2))),
                    _ => StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại"))
                };
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            }
        }
        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> DeleteASouvenir([FromRoute] Guid id)
        {
            try
            {
                var result = await _service.DeleteASouvenir(id);

                return result.Item1 switch
                {
                    1 => StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, "Không tồn tại!!!")),
                    2 => StatusCode(ConstantHttpStatus.OK, new ItemResponse<SouvenirRequest>("Đã xóa thành công!", _mapper.Map<SouvenirRequest>(result.Item2))),
                    _ => StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại"))
                };
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            }
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetASouvenirById([FromRoute] Guid id)
        {
            try
            {
                var souvenir = await _service.GetASouvenirById(id);

                return souvenir switch
                {
                    null => StatusCode(ConstantHttpStatus.NOT_FOUND, new ItemResponse<BookModel>(ConstantMessage.NotFound)),
                    not null => StatusCode(ConstantHttpStatus.OK, new ItemResponse<Souvenir>(ConstantMessage.Success, souvenir))
                };
            }
            catch (Exception ex)
            {

                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            };
        }
        [AllowAnonymous]
        [HttpPost("pagination-search")]
        public async Task<IActionResult> GetAllSouvenirsPagination(PaginatedRequest<SouvenirSearchRequest> request)
        {
            try
            {
                var souvenir = await _service.GetAllSouvenirsPagination(request != null && request.Result != null ? request.Result.SouvenirName : null, request != null ? request.PageNumber : 1, request != null ? request.PageSize : 10, request != null ? request.SortField : "CreatedDate", request != null && request.SortOrder == -1);

                return souvenir.Item2 switch
                {
                    0 => StatusCode(ConstantHttpStatus.NOT_FOUND, new PaginatedListResponse<SouvenirRequest>(ConstantMessage.Success, null)),
                    _ => StatusCode(ConstantHttpStatus.OK, new PaginatedListResponse<SouvenirRequest>(ConstantMessage.Success, _mapper.Map<List<SouvenirRequest>>(souvenir.Item1), souvenir.Item2, request != null ? request.PageNumber : 1, request != null ? request.PageSize : 10, request != null ? request.SortField : "CreatedDate", request != null && request.SortOrder != -1 ? 1 : -1))
                };
            }
            catch (Exception ex)
            {

                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            };
        }
    }
}
