using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.API.SearchModel;
using AIBookStreet.API.Tool.Constant;
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
    [Authorize]

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

        [HttpPost("search")]
        public async Task<IActionResult> Search(PaginatedRequest<PublisherSearchRequest> paginatedRequest)
        {
            try
            {
                var publisher = _mapper.Map<PublisherModel>(paginatedRequest.Result);
                var publishers = await _publisherService.Search(publisher, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField, paginatedRequest.SortOrder.Value);

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

        [HttpPost("add")]
        public async Task<IActionResult> Add(PublisherRequest publisherRequest)
        {
            try
            {
                var isPublisher = await _publisherService.Add(_mapper.Map<PublisherModel>(publisherRequest));

                return isPublisher switch
                {
                    true => Ok(new BaseResponse(isPublisher, ConstantMessage.Success)),
                    _ => Ok(new BaseResponse(isPublisher, ConstantMessage.Fail))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(PublisherRequest publisherRequest)
        {
            try
            {
                var publisherModel = _mapper.Map<PublisherModel>(publisherRequest);

                var isPublisher = await _publisherService.Update(publisherModel);

                return isPublisher switch
                {
                    true => Ok(new BaseResponse(isPublisher, ConstantMessage.Success)),
                    _ => Ok(new BaseResponse(isPublisher, ConstantMessage.Fail))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (id != Guid.Empty)
                {
                    var isPublisher = await _publisherService.Delete(id);

                    return isPublisher switch
                    {
                        true => Ok(new BaseResponse(isPublisher, ConstantMessage.Success)),
                        _ => Ok(new BaseResponse(isPublisher, ConstantMessage.Fail))
                    };
                }
                else
                {
                    return BadRequest("It's not empty");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
