using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.API.Tool.Constant;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
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

        //[Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddABookCategory([FromQuery] BookCategoryModel model)
        {
            try
            {
                var result = await _service.AddABookCategory(model);
                return result.Item2 == 1 ? Ok(new ItemResponse<BookCategory>("Đã tồn tại!")) :
                       result.Item2 == 2 ? Ok(new ItemResponse<BookCategory>("Đã thêm", result.Item1)) :
                                           Ok(new ItemResponse<BookCategory>("Đã xảy ra lỗi!!!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateABookCategory([FromRoute] Guid id, [FromQuery] BookCategoryModel model)
        {
            try
            {
                var result = await _service.UpdateABookCategory(id, model);

                return result.Item1 switch
                {
                    1 => Ok(new ItemResponse<BookCategory>("Không tồn tại!!!")),
                    2 => Ok(new ItemResponse<BookCategory>("Đã cập nhật thông tin!", result.Item2)),
                    _ => Ok(new ItemResponse<BookCategory>("Đã xảy ra lỗi, vui lòng kiểm tra lại"))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[Authorize]
        [HttpPut("delete/{id}")]
        public async Task<IActionResult> DeleteABookCategory([FromRoute] Guid id)
        {
            try
            {
                var result = await _service.DeleteABookCategory(id);

                return result.Item1 switch
                {
                    1 => Ok(new ItemResponse<BookCategory>("Không tồn tại!!!")),
                    2 => Ok(new ItemResponse<BookCategory>("Đã xóa thành công!", result.Item2)),
                    _ => Ok(new ItemResponse<BookCategory>("Đã xảy ra lỗi, vui lòng kiểm tra lại!!!"))
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
        [HttpGet("pagination-and-search")]
        public async Task<IActionResult> GetAllBookcategoriesPagination(string? key, Guid? bookID, Guid? categoryID, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            try
            {
                var bookCategories = await _service.GetAllBookCategoriesPagination(key, bookID, categoryID, pageNumber, pageSize, sortField, desc);

                return bookCategories.Item2 switch
                {
                    0 => Ok(new PaginatedListResponse<BookCategoryRequest>(ConstantMessage.Success, null)),
                    _ => Ok(new PaginatedListResponse<BookCategoryRequest>(ConstantMessage.Success, _mapper.Map<List<BookCategoryRequest>>(bookCategories.Item1), bookCategories.Item2, pageNumber != null ? (int)pageNumber : 1, pageSize != null ? (int)pageSize : 10, sortField, desc != null && (desc != false) ? 0 : 1))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            };
        }
        [HttpGet("get-all-by-element")]
        public async Task<IActionResult> GetBookCategoriesByElement(Guid? bookID, Guid? categoryID)
        {
            try
            {
                var bookCategories = await _service.GetBookCategoryByElement(bookID, categoryID);

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
