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
    [Route("api/publisher")]
    [ApiController]
    

    public class PublisherController : ControllerBase
    {
        private readonly IPublisherService _publisherService;
        private readonly IMapper _mapper;

        public PublisherController(IPublisherService publisherService, IMapper mapper)
        {
            _publisherService = publisherService;
            _mapper = mapper;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var publishers = await _publisherService.GetAll();

                return publishers switch
                {
                    null => Ok(new ItemListResponse<PublisherModel>(ConstantMessage.Fail, null)),
                    not null => Ok(new ItemListResponse<PublisherModel>(ConstantMessage.Success, publishers))
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
                var publishers = await _publisherService.GetAllPagination(paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField, paginatedRequest.SortOrder.Value);
                long totalOrigin = await _publisherService.GetTotalCount();
                return publishers switch
                {
                    null => Ok(new PaginatedListResponse<PublisherModel>(ConstantMessage.NotFound)),
                    not null => Ok(new PaginatedListResponse<PublisherModel>(ConstantMessage.Success, publishers, totalOrigin, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField))
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
                var publisherModel = await _publisherService.GetById(id);

                return publisherModel switch
                {
                    null => Ok(new ItemResponse<PublisherModel>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<PublisherModel>(ConstantMessage.Success, publisherModel))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }

        [HttpPost("search-pagination")]
        public async Task<IActionResult> SearchPagination(PaginatedRequest<PublisherSearchRequest> paginatedRequest)
        {
            try
            {
                var publisher = _mapper.Map<PublisherModel>(paginatedRequest.Result);
                var publishers = await _publisherService.SearchPagination(publisher, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField, paginatedRequest.SortOrder.Value);

                return publishers.Item1 switch
                {
                    null => Ok(new PaginatedListResponse<PublisherModel>(ConstantMessage.NotFound, publishers.Item1, publishers.Item2, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField)),
                    not null => Ok(new PaginatedListResponse<PublisherModel>(ConstantMessage.Success, publishers.Item1, publishers.Item2, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }

        [HttpPost("search-without-pagination")]
        public async Task<IActionResult> SearchWithoutPagination([FromBody] PublisherSearchRequest searchRequest)
        {
            try
            {
                var publisher = _mapper.Map<PublisherModel>(searchRequest);
                var publishers = await _publisherService.SearchWithoutPagination(publisher);

                return publishers switch
                {
                    null => Ok(new ItemListResponse<PublisherModel>(ConstantMessage.NotFound, null)),
                    not null => Ok(new ItemListResponse<PublisherModel>(ConstantMessage.Success, publishers))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> Add([FromForm] PublisherRequest publisherRequest)
        {
            try
            {
                var publisherModel = _mapper.Map<PublisherModel>(publisherRequest);
                publisherModel.MainImageFile = publisherRequest.MainImageFile;
                publisherModel.AdditionalImageFiles = publisherRequest.AdditionalImageFiles;

                var (result, message) = await _publisherService.Add(publisherModel);
                return result switch
                {
                    null => StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, message)),
                    not null => StatusCode(ConstantHttpStatus.CREATED, new ItemResponse<PublisherModel>(message, result))
                };
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            }
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> Update(Guid id, [FromForm] PublisherRequest publisherRequest)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ConstantMessage.EmptyId));
                }

                var publisherModel = _mapper.Map<PublisherModel>(publisherRequest);
                publisherModel.Id = id;
                publisherModel.MainImageFile = publisherRequest.MainImageFile;
                publisherModel.AdditionalImageFiles = publisherRequest.AdditionalImageFiles;

                var (result, message) = await _publisherService.Update(publisherModel);
                return result switch
                {
                    null => StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, message)),
                    not null => StatusCode(ConstantHttpStatus.OK, new ItemResponse<PublisherModel>(message, result))
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

                var (result, message) = await _publisherService.Delete(id);
                return result switch
                {
                    null => StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, message)),
                    not null => StatusCode(ConstantHttpStatus.OK, new ItemResponse<PublisherModel>(message, result))
                };
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            }
        }
    }
}
