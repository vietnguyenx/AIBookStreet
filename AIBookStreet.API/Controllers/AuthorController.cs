using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
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
        public async Task<IActionResult> AddAnAuthor([FromQuery]AuthorModel author)
        {
            try
            {
                var result = await _service.AddAnAuthor(author);
                return result == null ? Ok(new ItemResponse<Author>(ConstantMessage.Fail)) : Ok(new ItemResponse<Author>("Đã thêm tác giả",result));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateAnAuthor([FromRoute]Guid id, [FromQuery]AuthorModel author)
        {
            try
            {
                var result = await _service.UpdateAnAuthor(id, author);

                return result.Item1 switch
                {
                    1 => Ok(new ItemResponse<Author>("Tác giả không tồn tại!!!")),
                    2 => Ok(new ItemResponse<Author>("Đã cập nhật thông tin!", result.Item2)),
                    _ => Ok(new ItemResponse<Author>("Đã xảy ra lỗi, vui lòng kiểm tra lại"))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPut("delete/{id}")]
        public async Task<IActionResult> DeleteAnAuthor([FromRoute]Guid id)
        {
            try
            {
                var result = await _service.DeleteAnAuthor(id);

                return result.Item1 switch
                {
                    1 => Ok(new ItemResponse<Author>("Tác giả không tồn tại!!!", result.Item2)),
                    2 => Ok(new ItemResponse<Author>("Đã xóa thành công!", result.Item2)),
                    _ => Ok(new ItemResponse<Author>("Đã xảy ra lỗi, vui lòng kiểm tra lại!!!"))
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
        [HttpGet("get-all-active")]
        public async Task<IActionResult> GetAllActiveAuthors()
        {
            try
            {
                var authors = await _service.GetAllActiveAuthors();

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
        [HttpGet("pagination-and-search")]
        public async Task<IActionResult> GetAllAuthorPagination(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            try
            {
                var authors = await _service.GetAllAuthorsPagination(key, pageNumber, pageSize, sortField, desc);

                return authors.Item2 switch
                {
                    0 => Ok(new PaginatedListResponse<AuthorRequest>(ConstantMessage.Success, null)),
                    _ => Ok(new PaginatedListResponse<AuthorRequest>(ConstantMessage.Success, _mapper.Map<List<AuthorRequest>>(authors.Item1), authors.Item2, pageNumber != null ? (int)pageNumber : 1, pageSize != null ? (int)pageSize : 10, sortField, desc != null && (desc != false) ? 0 : 1))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
    }
}
