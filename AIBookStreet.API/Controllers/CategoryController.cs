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
    public class CategoryController(ICategoryService service, IMapper mapper) : ControllerBase
    {
        private readonly ICategoryService _service = service;
        private readonly IMapper _mapper = mapper;

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddACategory(CategoryModel model)
        {
            try
            {
                var result = await _service.AddACategory(model);
                return result == null ? Ok(new BaseResponse(false, "Đã xảy ra lỗi!!!")) : Ok(new BaseResponse(true, "Đã thêm"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateACategory(CategoryModel model)
        {
            try
            {
                var result = await _service.UpdateACategory(model.Id, model);

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
        public async Task<IActionResult> DeleteACategory([FromRoute] Guid id)
        {
            try
            {
                var result = await _service.DeleteACategory(id);

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
        public async Task<IActionResult> GetACategoryById([FromRoute] Guid id)
        {
            try
            {
                var category = await _service.GetACategoryById(id);

                return category switch
                {
                    null => Ok(new ItemResponse<Category>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<Category>(ConstantMessage.Success, category))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [AllowAnonymous]
        [HttpPost("search-not-pagination")]
        public async Task<IActionResult> GetAllActiveCategories(CategorySearchRequest request)
        {
            try
            {
                var categories = await _service.GetAllActiveCategories(request.CategoryName);

                return categories switch
                {
                    null => Ok(new ItemListResponse<CategoryRequest>(ConstantMessage.Success, null)),
                    not null => Ok(new ItemListResponse<CategoryRequest>(ConstantMessage.Success, _mapper.Map<List<CategoryRequest>>(categories)))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpPost("pagination-and-search")]
        public async Task<IActionResult> GetAllCategoriesPagination(PaginatedRequest<CategorySearchRequest> request)
        {
            try
            {
                var categories = await _service.GetAllCategoriesPagination(request.Result.CategoryName, request.PageNumber, request.PageSize, request.SortField, request.SortOrder == 0);

                return categories.Item2 switch
                {
                    0 => Ok(new PaginatedListResponse<CategoryRequest>(ConstantMessage.Success, null)),
                    _ => Ok(new PaginatedListResponse<CategoryRequest>(ConstantMessage.Success, _mapper.Map<List<CategoryRequest>>(categories.Item1), categories.Item2, request.PageNumber, request.PageSize, request.SortField, request.SortOrder))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
    }
}
