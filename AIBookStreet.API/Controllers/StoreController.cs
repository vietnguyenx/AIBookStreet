using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.API.SearchModel;
using AIBookStreet.Services.Common;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AIBookStreet.Services.Services.Service;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AIBookStreet.API.Controllers
{
    [Route("api/stores")]
    [ApiController]
    

    public class StoreController : ControllerBase
    {
        private readonly IStoreService _storeService;
        private readonly IMapper _mapper;

        public StoreController(IStoreService storeService, IMapper mapper)
        {
            _storeService = storeService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var stores = await _storeService.GetAll();

                return stores switch
                {
                    null => Ok(new ItemListResponse<StoreModel>(ConstantMessage.Fail, null)),
                    not null => Ok(new ItemListResponse<StoreModel>(ConstantMessage.Success, stores))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("paginated")]
        public async Task<IActionResult> GetAllPagination(PaginatedRequest paginatedRequest)
        {
            try
            {
                var stores = await _storeService.GetAllPagination(paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField, paginatedRequest.SortOrder.Value);
                long totalOrigin = await _storeService.GetTotalCount();
                return stores switch
                {
                    null => Ok(new PaginatedListResponse<StoreModel>(ConstantMessage.NotFound)),
                    not null => Ok(new PaginatedListResponse<StoreModel>(ConstantMessage.Success, stores, totalOrigin, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("Id is empty");
                }
                var storeModel = await _storeService.GetById(id);

                return storeModel switch
                {
                    null => Ok(new ItemResponse<StoreModel>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<StoreModel>(ConstantMessage.Success, storeModel))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }

        [HttpPost("search/paginated")]
        public async Task<IActionResult> SearchPagination(PaginatedRequest<StoreSearchRequest> paginatedRequest)
        {
            try
            {
                var store = _mapper.Map<StoreModel>(paginatedRequest.Result);
                var stores = await _storeService.SearchPagination(store, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField, paginatedRequest.SortOrder.Value);

                return stores.Item1 switch
                {
                    null => Ok(new PaginatedListResponse<StoreModel>(ConstantMessage.NotFound, stores.Item1, stores.Item2, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField)),
                    not null => Ok(new PaginatedListResponse<StoreModel>(ConstantMessage.Success, stores.Item1, stores.Item2, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchWithoutPagination(StoreSearchRequest searchRequest)
        {
            try
            {
                var storeModel = _mapper.Map<StoreModel>(searchRequest);
                var stores = await _storeService.SearchWithoutPagination(storeModel);

                return stores == null
                    ? Ok(new ItemListResponse<StoreModel>(ConstantMessage.NotFound, null))
                    : Ok(new ItemListResponse<StoreModel>(ConstantMessage.Success, stores));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add([FromForm] StoreRequest storeRequest)
        {
            try
            {
                var storeModel = _mapper.Map<StoreModel>(storeRequest);
                storeModel.MainImageFile = storeRequest.MainImageFile;
                storeModel.AdditionalImageFiles = storeRequest.AdditionalImageFiles;

                var (result, message) = await _storeService.Add(storeModel);
                return result switch
                {
                    null => StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, message)),
                    not null => StatusCode(ConstantHttpStatus.CREATED, new ItemResponse<StoreModel>(message, result))
                };
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] StoreRequest storeRequest)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ConstantMessage.EmptyId));
                }

                var storeModel = _mapper.Map<StoreModel>(storeRequest);
                storeModel.Id = id;
                storeModel.MainImageFile = storeRequest.MainImageFile;
                storeModel.AdditionalImageFiles = storeRequest.AdditionalImageFiles;

                var (result, message) = await _storeService.Update(storeModel);
                return result switch
                {
                    null => StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, message)),
                    not null => StatusCode(ConstantHttpStatus.OK, new ItemResponse<StoreModel>(message, result))
                };
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            }
        }

        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ConstantMessage.EmptyId));
                }

                var (result, message) = await _storeService.Delete(id);
                return result switch
                {
                    null => StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, message)),
                    not null => StatusCode(ConstantHttpStatus.OK, new ItemResponse<StoreModel>(message, result))
                };
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            }
        }

        [HttpGet("stats/total")]
        public async Task<IActionResult> GetTotalStoreCount()
        {
            try
            {
                var result = await _storeService.GetTotalStoreCountWithChangePercent();
                return Ok(new
                {
                    success = true,
                    total = result.totalCount,
                    currentMonthPercentChange = Math.Abs(result.percentChange),
                    changeDirection = result.percentChange > 0 ? "increase" : (result.percentChange < 0 ? "decrease" : "unchanged")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.INTERNAL_SERVER_ERROR, 
                    new BaseResponse(false, $"Lỗi: {ex.Message}"));
            }
        }

        [HttpGet("{storeId}/stats/products")]
        public async Task<IActionResult> GetStoreProductCounts(Guid storeId)
        {
            try
            {
                if (storeId == Guid.Empty)
                {
                    return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ConstantMessage.EmptyId));
                }

                var result = await _storeService.GetStoreProductCountsByCategory(storeId);
                return Ok(new
                {
                    success = true,
                    bookCount = result.bookCount,
                    souvenirCount = result.souvenirCount,
                    totalCount = result.totalCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.INTERNAL_SERVER_ERROR, 
                    new BaseResponse(false, $"Lỗi: {ex.Message}"));
            }
        }
    }
}
