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
    [Route("api/authors")]
    [ApiController]
    public class AuthorController(IAuthorService service, IMapper mapper) : ControllerBase
    {
        private readonly IAuthorService _service = service;
        private readonly IMapper _mapper = mapper;

        [Authorize]
        [HttpPost("")]
        public async Task<IActionResult> AddAnAuthor([FromForm]AuthorModel author)
        {
            try
            {
                var result = await _service.AddAnAuthor(author);
                return result == null ? BadRequest(new BaseResponse(false, "Đã xảy ra lỗi!!!")) : Ok(new ItemResponse<AuthorRequest>("Đã thêm tác giả", _mapper.Map<AuthorRequest>(result)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAnAuthor([FromRoute]Guid id, [FromForm]AuthorModel author)
        {
            try
            {
                var result = await _service.UpdateAnAuthor(id, author);

                return result.Item1 switch
                {
                    1 => BadRequest(new BaseResponse(false, "Tác giả không tồn tại!!!")),
                    2 => Ok(new ItemResponse<AuthorRequest>("Đã cập nhật thông tin!", _mapper.Map<AuthorRequest>(result.Item2))),
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
        public async Task<IActionResult> DeleteAnAuthor([FromRoute]Guid id)
        {
            try
            {
                var result = await _service.DeleteAnAuthor(id);

                return result.Item1 switch
                {
                    1 => BadRequest(new BaseResponse(false, "Tác giả không tồn tại!!!")),
                    2 => Ok(new ItemResponse<AuthorRequest>("Đã xóa thành công!", _mapper.Map<AuthorRequest>(result.Item2))),
                    _ => BadRequest(new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại!!!"))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAnAuthorById([FromRoute]Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("Id is empty");
                }
                var author = await _service.GetAnAuthorById(id);

                return author switch
                {
                    null => BadRequest(new ItemResponse<Author>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<Author>(ConstantMessage.Success, author))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [AllowAnonymous]
        [HttpPost("search")]
        public async Task<IActionResult> GetAllActiveAuthors(AuthorSearchRequest? request)
        {
            try
            {
                var authors = request != null ? await _service.GetAllActiveAuthors(request.AuthorName, request.CategoryId) :
                                                await _service.GetAllActiveAuthors(null, null);

                return authors switch
                {
                    null => BadRequest(new ItemListResponse<AuthorRequest>(ConstantMessage.Success, null)),
                    not null => Ok(new ItemListResponse<AuthorRequest>(ConstantMessage.Success, _mapper.Map<List<AuthorRequest>>(authors)))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpPost("search/paginated")]
        public async Task<IActionResult> GetAllAuthorPagination(PaginatedRequest<AuthorSearchRequest> request)
        {
            try
            {
                var authors = await _service.GetAllAuthorsPagination(request != null && request.Result != null ? request.Result.AuthorName : null, request != null && request.Result != null ? request.Result.CategoryId : null, request != null ? request.PageNumber : 1, request != null ? request.PageSize : 10, request != null ? request.SortField : "CreatedDate", request != null && request.SortOrder == -1);

                return authors.Item2 switch
                {
                    0 => BadRequest(new PaginatedListResponse<AuthorRequest>(ConstantMessage.Success, null)),
                    _ => Ok(new PaginatedListResponse<AuthorRequest>(ConstantMessage.Success, _mapper.Map<List<AuthorRequest>>(authors.Item1), authors.Item2, request != null ? request.PageNumber : 1, request != null ? request.PageSize : 10, request != null ? request.SortField : "CreatedDate", request != null && request.SortOrder != 0 ? -1 : 1))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
    }
}
