﻿using AIBookStreet.API.RequestModel;
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
    [Route("api/categories")]
    [ApiController]
    public class CategoryController(ICategoryService service, IMapper mapper) : ControllerBase
    {
        private readonly ICategoryService _service = service;
        private readonly IMapper _mapper = mapper;

        [Authorize]
        [HttpPost("")]
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
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateACategory([FromRoute] Guid id, CategoryModel model)
        {
            try
            {
                var result = await _service.UpdateACategory(id, model);

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
        [HttpPatch("{id}")]
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
        [HttpGet("{id}")]
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
        [HttpGet("")]
        public async Task<IActionResult> GetAllActiveCategories()
        {
            try
            {
                var categories = await _service.GetAllActiveCategories(null,null);

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
        [HttpPost("search")]
        public async Task<IActionResult> GetAllActiveAndSearchCategories(CategorySearchRequest? request)
        {
            try
            {
                var categories = request != null ? await _service.GetAllActiveCategories(request.CategoryName, request.AuthorId):
                                                    await _service.GetAllActiveCategories(null, null);

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
        [HttpPost("search/paginated")]
        public async Task<IActionResult> GetAllCategoriesPagination(PaginatedRequest<CategorySearchRequest> request)
        {
            try
            {
                var categories = await _service.GetAllCategoriesPagination(request != null && request.Result != null ? request.Result.CategoryName : null, request != null && request.Result != null ? request.Result.AuthorId : null, request != null ? request.PageNumber : 1, request != null? request.PageSize : 10, request != null? request.SortField : "CreatedDate", request != null && request.SortOrder == -1);

                return categories.Item2 switch
                {
                    0 => Ok(new PaginatedListResponse<CategoryRequest>(ConstantMessage.Success, null)),
                    _ => Ok(new PaginatedListResponse<CategoryRequest>(ConstantMessage.Success, _mapper.Map<List<CategoryRequest>>(categories.Item1), categories.Item2, request != null ? request.PageNumber: 1, request != null ? request.PageSize : 10, request != null? request.SortField : "CreatedDate", request != null ? request.SortOrder : 1))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
    }
}
