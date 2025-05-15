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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Drawing.Printing;

namespace AIBookStreet.API.Controllers
{
    [Route("api/book-authors")]
    [ApiController]
    public class BookAuthorController(IBookAuthorService service, IMapper mapper) : ControllerBase
    {
        private readonly IBookAuthorService _service = service;
        private readonly IMapper _mapper = mapper;

        [Authorize]
        [HttpPost("")]
        public async Task<IActionResult> AddABookAuthor(BookAuthorModel model)
        {
            try
            {
                var result = await _service.AddABookAuthor(model);
                return result.Item2 == 1 ? BadRequest(new BaseResponse(false, "Đã tồn tại!")) : 
                       result.Item2 == 2 ? Ok(new BaseResponse(true, "Đã thêm")) :
                                           BadRequest(new BaseResponse(false, "Đã xảy ra lỗi!!!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[Authorize]
        //[HttpPut("update/authorId={authorId}&bookId={bookId}")]
        //public async Task<IActionResult> UpdateABookAuthor([FromRoute] Guid authorId,[FromRoute] Guid bookId)
        //{
        //    try
        //    {
        //        var model = new BookAuthorModel
        //        {
        //            AuthorId = authorId,
        //            BookId = bookId
        //        };
        //        var result = await _service.UpdateABookAuthor(model);

        //        return result.Item1 switch
        //        {
        //            1 => Ok(new BaseResponse(false, "Không tồn tại!!!")),
        //            2 => Ok(new BaseResponse(true, "Đã cập nhật thông tin!")),
        //            _ => Ok(new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại"))
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
        [Authorize]
        [HttpPatch("{bookId}/{authorId}")]
        public async Task<IActionResult> DeleteABookAuthor([FromRoute] Guid authorId,[FromRoute] Guid bookId)
        {
            try
            {
                var model = new BookAuthorModel
                {
                    AuthorId = authorId,
                    BookId = bookId
                };
                var result = await _service.DeleteABookAuthor(model);

                return result.Item1 switch
                {
                    1 => NotFound(new BaseResponse(false, "Không tồn tại!!!")),
                    2 => Ok(new BaseResponse(true, "Đã xóa thông tin!")),
                    _ => BadRequest(new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại"))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[AllowAnonymous]
        //[HttpGet("/{bookId}/{authorId}")]
        //public async Task<IActionResult> GetABookAuthorById([FromRoute] Guid bookId, [FromRoute] Guid authorId)
        //{
        //    try
        //    {
        //        var model = new BookAuthorModel
        //        {
        //            AuthorId = authorId,
        //            BookId = bookId
        //        };
        //        var bookAuthor = await _service.GetABookAuthorById(model);

        //        return bookAuthor switch
        //        {
        //            null => Ok(new ItemResponse<BookAuthor>(ConstantMessage.NotFound)),
        //            not null => Ok(new ItemResponse<BookAuthor>(ConstantMessage.Success, bookAuthor))
        //        };
        //    }
        //    catch (Exception ex)
        //    {

        //        return BadRequest(ex.Message);
        //    };
        //}
        [AllowAnonymous]
        [HttpGet("non-deleted")]
        public async Task<IActionResult> GetAllActiveBookAuthors()
        {
            try
            {
                var bookAuthors = await _service.GetAllActiveBookAuthors();

                if (bookAuthors == null)
                {
                    return NotFound(new ItemListResponse<BookAuthorRequest>(ConstantMessage.Success, null));
                }
                
                return Ok(new ItemListResponse<BookAuthorRequest>(ConstantMessage.Success, _mapper.Map<List<BookAuthorRequest>>(bookAuthors)));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpPost("search/paginated")]
        public async Task<IActionResult> GetAllBookAuthorsPagination(PaginatedRequest<BookAuthorSearchRequest> request)
        {
            try
            {
                var bookAuthors = await _service.GetAllBookAuthorsPagination(request != null && request.Result != null ? request.Result.Key : null, request != null && request.Result != null ? request.Result.BookId : null, request != null && request.Result != null ? request.Result.AuthorId : null, request != null ? request.PageNumber : 1, request != null ? request.PageSize : 10, request != null ? request.SortField : "CreatedDate", request != null && request.SortOrder == -1);

                return bookAuthors.Item2 switch
                {
                    0 => NotFound(new PaginatedListResponse<BookAuthorRequest>(ConstantMessage.Success, null)),
                    _ => Ok(new PaginatedListResponse<BookAuthorRequest>(ConstantMessage.Success, _mapper.Map<List<BookAuthorRequest>>(bookAuthors.Item1), bookAuthors.Item2, request != null ? request.PageNumber : 1, request != null ? request.PageSize: 10, request != null ? request.SortField : "CreatedDate", request != null ? request.SortOrder : 1))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            };
        }
        [AllowAnonymous]
        [HttpPost("filter")]
        public async Task<IActionResult> GetABookAuthorByElement(BookAuthorSearchRequest request)
        {
            try
            {
                var bookAuthors = await _service.GetBookAuthorByElement(request.BookId, request.AuthorId);

                return bookAuthors switch
                {
                    null => NotFound(new ItemListResponse<BookAuthorRequest>(ConstantMessage.Success, null)),
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
