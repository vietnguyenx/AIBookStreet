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
    public class CategoryController(ICategoryService service, IMapper mapper) : ControllerBase
    {
        private readonly ICategoryService _service = service;
        private readonly IMapper _mapper = mapper;

        //[Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddACategory([FromQuery] CategoryModel model)
        {
            try
            {
                var result = await _service.AddACategory(model);
                return result == null ? Ok(new ItemResponse<Category>(ConstantMessage.Fail)) : Ok(new ItemResponse<Category>("Đã thêm", result));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateACategory([FromRoute] Guid id, [FromQuery] CategoryModel model)
        {
            try
            {
                var result = await _service.UpdateACategory(id, model);

                return result.Item1 switch
                {
                    1 => Ok(new ItemResponse<Category>("Không tồn tại!!!")),
                    2 => Ok(new ItemResponse<Category>("Đã cập nhật thông tin!", result.Item2)),
                    _ => Ok(new ItemResponse<Category>("Đã xảy ra lỗi, vui lòng kiểm tra lại"))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[Authorize]
        [HttpPut("delete/{id}")]
        public async Task<IActionResult> DeleteACategory([FromRoute] Guid id)
        {
            try
            {
                var result = await _service.DeleteACategory(id);

                return result.Item1 switch
                {
                    1 => Ok(new ItemResponse<Category>("Không tồn tại!!!")),
                    2 => Ok(new ItemResponse<Category>("Đã xóa thành công!", result.Item2)),
                    _ => Ok(new ItemResponse<Category>("Đã xảy ra lỗi, vui lòng kiểm tra lại!!!"))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
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
        [HttpGet("get-all-active")]
        public async Task<IActionResult> GetAllActiveCategories()
        {
            try
            {
                var categories = await _service.GetAllActiveCategories();

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
        [HttpGet("pagination-and-search")]
        public async Task<IActionResult> GetAllCategoriesPagination(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            try
            {
                var categories = await _service.GetAllCategoriesPagination(key, pageNumber, pageSize, sortField, desc);

                return categories.Item2 switch
                {
                    0 => Ok(new PaginatedListResponse<CategoryRequest>(ConstantMessage.Success, null)),
                    _ => Ok(new PaginatedListResponse<CategoryRequest>(ConstantMessage.Success, _mapper.Map<List<CategoryRequest>>(categories.Item1), categories.Item2, pageNumber != null ? (int)pageNumber : 1, pageSize != null ? (int)pageSize : 10, sortField, desc != null && (desc != false) ? 0 : 1))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
    }
}
