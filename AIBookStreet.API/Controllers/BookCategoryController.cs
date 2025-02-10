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
    public class BookCategoryController(IBookCategoryService service, IMapper mapper) : ControllerBase
    {
        private readonly IBookCategoryService _service = service;
        private readonly IMapper _mapper = mapper;

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddABookCategory(BookCategoryModel model)
        {
            try
            {
                var result = await _service.AddABookCategory(model);
                return result.Item2 == 1 ? Ok(new BaseResponse(false, "Đã tồn tại!")) :
                        result.Item2 == 2 ? Ok(new BaseResponse(true, "Đã thêm")) :
                                            Ok(new BaseResponse(false, "Đã xảy ra lỗi!!!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateABookCategory(BookCategoryModel model)
        {
            try
            {
                var result = await _service.UpdateABookCategory(model.Id, model);

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
        [HttpPut("delete")]
        public async Task<IActionResult> DeleteABookCategory(Guid id)
        {
            try
            {
                var result = await _service.DeleteABookCategory(id);

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
        public async Task<IActionResult> GetABookCategoryById([FromRoute] Guid id)
        {
            try
            {
                var bookCategory = await _service.GetABookCategoryById(id);

                return bookCategory switch
                {
                    null => Ok(new ItemResponse<BookCategory>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<BookCategory>(ConstantMessage.Success, bookCategory))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [HttpGet("get-all-active")]
        public async Task<IActionResult> GetAllActiveBookCategories()
        {
            try
            {
                var bookCategories = await _service.GetAllActiveBookCategories();

                return bookCategories switch
                {
                    null => Ok(new ItemListResponse<BookCategoryRequest>(ConstantMessage.Success, null)),
                    not null => Ok(new ItemListResponse<BookCategoryRequest>(ConstantMessage.Success, _mapper.Map<List<BookCategoryRequest>>(bookCategories)))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("pagination-and-search")]
        public async Task<IActionResult> GetAllBookcategoriesPagination(PaginatedRequest<BookCategorySearchRequest> request)
        {
            try
            {
                var bookCategories = await _service.GetAllBookCategoriesPagination(request.Result.Key, request.Result.BookId, request.Result.CategoryId, request.PageNumber, request.PageSize, request.SortField, request.SortOrder == 0);

                return bookCategories.Item2 switch
                {
                    0 => Ok(new PaginatedListResponse<BookCategoryRequest>(ConstantMessage.Success, null)),
                    _ => Ok(new PaginatedListResponse<BookCategoryRequest>(ConstantMessage.Success, _mapper.Map<List<BookCategoryRequest>>(bookCategories.Item1), bookCategories.Item2, request.PageNumber, request.PageSize, request.SortField, request.SortOrder))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            };
        }
        [HttpPost("get-all-by-element")]
        public async Task<IActionResult> GetBookCategoriesByElement(BookCategorySearchRequest request)
        {
            try
            {
                var bookCategories = await _service.GetBookCategoryByElement(request.BookId, request.CategoryId);

                return bookCategories switch
                {
                    null => Ok(new ItemListResponse<BookCategoryRequest>(ConstantMessage.Success, null)),
                    _ => Ok(new ItemListResponse<BookCategoryRequest>(ConstantMessage.Success, _mapper.Map<List<BookCategoryRequest>>(bookCategories)))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            };
        }
    }
}
