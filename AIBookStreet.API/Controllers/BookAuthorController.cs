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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Drawing.Printing;

namespace AIBookStreet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookAuthorController(IBookAuthorService service, IMapper mapper) : ControllerBase
    {
        private readonly IBookAuthorService _service = service;
        private readonly IMapper _mapper = mapper;

        //[Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddABookAuthor([FromQuery] BookAuthorModel model)
        {
            try
            {
                var result = await _service.AddABookAuthor(model);
                return result.Item2 == 1 ? Ok(new BaseResponse(false, "Đã tồn tại!")) : 
                       result.Item2 == 2 ? Ok(new BaseResponse(true, "Đã thêm")) :
                                           Ok(new BaseResponse(false, "Đã xảy ra lỗi!!!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateABookAuthor([FromRoute] Guid id, [FromQuery] BookAuthorModel model)
        {
            try
            {
                var result = await _service.UpdateABookAuthor(id, model);

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
        public async Task<IActionResult> DeleteABookAuthor([FromRoute] Guid id)
        {
            try
            {
                var result = await _service.DeleteABookAuthor(id);

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
        public async Task<IActionResult> GetABookAuthorById([FromRoute] Guid id)
        {
            try
            {
                var bookAuthor = await _service.GetABookAuthorById(id);

                return bookAuthor switch
                {
                    null => Ok(new ItemResponse<BookAuthor>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<BookAuthor>(ConstantMessage.Success, bookAuthor))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [HttpGet("get-all-active")]
        public async Task<IActionResult> GetAllActiveBookAuthors()
        {
            try
            {
                var bookAuthors = await _service.GetAllActiveBookAuthors();

                if (bookAuthors == null)
                {
                    return Ok(new ItemListResponse<BookAuthorRequest>(ConstantMessage.Success, null));
                }
                
                return Ok(new ItemListResponse<BookAuthorRequest>(ConstantMessage.Success, _mapper.Map<List<BookAuthorRequest>>(bookAuthors)));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("pagination-and-search")]
        public async Task<IActionResult> GetAllBookAuthorsPagination(string? key, Guid? bookID, Guid? authorID, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            try
            {
                var bookAuthors = await _service.GetAllBookAuthorsPagination(key, bookID, authorID, pageNumber, pageSize, sortField, desc);

                return bookAuthors.Item2 switch
                {
                    0 => Ok(new PaginatedListResponse<BookAuthorRequest>(ConstantMessage.Success, null)),
                    _ => Ok(new PaginatedListResponse<BookAuthorRequest>(ConstantMessage.Success, _mapper.Map<List<BookAuthorRequest>>(bookAuthors.Item1), bookAuthors.Item2, pageNumber != null ? (int)pageNumber : 1, pageSize != null ? (int)pageSize : 10, sortField, desc != null && (desc != false) ? 0 : 1))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            };
        }
        [HttpGet("get-all-by-element")]
        public async Task<IActionResult> GetABookAuthorByElement(Guid? bookID, Guid? authorID)
        {
            try
            {
                var bookAuthors = await _service.GetBookAuthorByElement(bookID, authorID);

                return bookAuthors switch
                {
                    null => Ok(new ItemListResponse<BookAuthorRequest>(ConstantMessage.Success, null)),
                    _ => Ok(new ItemListResponse<BookAuthorRequest>(ConstantMessage.Success, _mapper.Map<List<BookAuthorRequest>>(bookAuthors)))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            };
        }
    }
}
