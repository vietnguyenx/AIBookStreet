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
    [Route("api/store")]
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

        [HttpGet("get-all")]
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

        [HttpPost("get-all-pagination")]
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

        [HttpGet("get-by-id/{id}")]
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

        [HttpPost("search-pagination")]
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

        [HttpPost("search-without-pagination")]
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
        [HttpPost("add")]
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
        [HttpPut("update")]
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
        [HttpDelete("delete/{id}")]
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
    }
}
