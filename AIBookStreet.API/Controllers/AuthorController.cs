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
    public class AuthorController(IAuthorService service, IMapper mapper) : ControllerBase
    {
        private readonly IAuthorService _service = service;
        private readonly IMapper _mapper = mapper;

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddAnAuthor(AuthorModel author)
        {
            try
            {
                var result = await _service.AddAnAuthor(author);
                return result == null ? Ok(new BaseResponse(false, "Đã xảy ra lỗi!!!")) : Ok(new BaseResponse(true, "Đã thêm tác giả"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAnAuthor(AuthorModel author)
        {
            try
            {
                var result = await _service.UpdateAnAuthor(author.Id, author);

                return result.Item1 switch
                {
                    1 => Ok(new BaseResponse(false, "Tác giả không tồn tại!!!")),
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
        [HttpPut("delete")]
        public async Task<IActionResult> DeleteAnAuthor(Guid id)
        {
            try
            {
                var result = await _service.DeleteAnAuthor(id);

                return result.Item1 switch
                {
                    1 => Ok(new BaseResponse(false, "Tác giả không tồn tại!!!")),
                    2 => Ok(new BaseResponse(true, "Đã xóa thành công!")),
                    _ => Ok(new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại!!!"))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("get-by-id/{id}")]
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
                    null => Ok(new ItemResponse<Author>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<Author>(ConstantMessage.Success, author))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [HttpPost("search-not-pagination")]
        public async Task<IActionResult> GetAllActiveAuthors(AuthorSearchRequest request)
        {
            try
            {
                var authors = await _service.GetAllActiveAuthors(request.AuthorName);

                return authors switch
                {
                    null => Ok(new ItemListResponse<AuthorRequest>(ConstantMessage.Success, null)),
                    not null => Ok(new ItemListResponse<AuthorRequest>(ConstantMessage.Success, _mapper.Map<List<AuthorRequest>>(authors)))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("pagination-and-search")]
        public async Task<IActionResult> GetAllAuthorPagination(PaginatedRequest<AuthorSearchRequest> request)
        {
            try
            {
                var authors = await _service.GetAllAuthorsPagination(request != null && request.Result != null ? request.Result.AuthorName : null, request.PageNumber, request.PageSize, request.SortField, request.SortOrder == 0);

                return authors.Item2 switch
                {
                    0 => Ok(new PaginatedListResponse<AuthorRequest>(ConstantMessage.Success, null)),
                    _ => Ok(new PaginatedListResponse<AuthorRequest>(ConstantMessage.Success, _mapper.Map<List<AuthorRequest>>(authors.Item1), authors.Item2, request.PageNumber, request.PageSize, request.SortField, request.SortOrder != null && (request.SortOrder != 0) ? 0 : 1))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
    }
}
