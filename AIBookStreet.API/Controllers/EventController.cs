using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.API.SearchModel;
using AIBookStreet.API.Tool.Constant;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AIBookStreet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController(IEventService service, IMapper mapper) : ControllerBase
    {
        private readonly IEventService _service = service;
        private readonly IMapper _mapper = mapper;

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddAnEvent(EventModel model)
        {
            try
            {
                var result = await _service.AddAnEvent(model);
                return result.Item1 == 1 ? Ok(new BaseResponse(false, "Đã có sự kiện trong thời gian trên đường sách này")) 
                     : result.Item1 == 2 ? Ok(new BaseResponse(true, "Đã thêm"))
                     :                     Ok(new BaseResponse(false, "Đã xảy ra lỗi"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAnEvent(EventModel model)
        {
            try
            {
                var result = await _service.UpdateAnEvent(model.Id, model);

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
        public async Task<IActionResult> DeleteAnEvent(Guid id)
        {
            try
            {
                var result = await _service.DeleteAnEvent(id);

                return result.Item1 switch
                {
                    1 => Ok(new BaseResponse(false, "Không tồn tại!!!")),
                    2 => Ok(new BaseResponse(true, "Đã xóa thành công!")),
                    _ => Ok(new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại!!!"))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetAnEventById([FromRoute] Guid id)
        {
            try
            {
                var evt = await _service.GetAnEventById(id);

                return evt switch
                {
                    null => Ok(new ItemResponse<Event>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<Event>(ConstantMessage.Success, evt))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [AllowAnonymous]
        [HttpGet("get-event-coming")]
        public async Task<IActionResult> GetEventsComing(int number)
        {
            try
            {
                var events = await _service.GetEventComing(number);

                return events switch
                {
                    null => Ok(new ItemListResponse<EventRequest>(ConstantMessage.Success, null)),
                    not null => Ok(new ItemListResponse<EventRequest>(ConstantMessage.Success, _mapper.Map<List<EventRequest>>(events)))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpPost("pagination-and-search")]
        public async Task<IActionResult> GetAllEventsPagination(PaginatedRequest<EventSearchRequest> request)
        {
            try
            {
                var events = await _service.GetAllEventsPagination(request.Result.Key, request.Result.StartDate, request.Result.EndDate, request.Result.ZoneId, request.PageNumber, request.PageSize, request.SortField, request.SortOrder == 0);

                return events.Item2 switch
                {
                    0 => Ok(new PaginatedListResponse<EventRequest>(ConstantMessage.Success, null)),
                    _ => Ok(new PaginatedListResponse<EventRequest>(ConstantMessage.Success, _mapper.Map<List<EventRequest>>(events.Item1), events.Item2, request.PageNumber, request.PageSize, request.SortField, request.SortOrder))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [AllowAnonymous]
        [HttpPost("get-date-have-event-in-month")]
        public async Task<IActionResult> GetEventDatesInMonth(int? month)
        {
            try
            {
                if (month < 1 ||  month > 12)
                {
                    return Ok("Tháng phải trong khoảng 1 - 12 !!!");
                }
                var dates = await _service.GetEventDatesInMonth(month);

                return dates switch
                {
                    null => Ok(new List<DateOnly>()),
                    not null => Ok(dates)
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpPost("get-events-by-date")]
        public async Task<IActionResult> GetEventsByDate(DateTime? date)
        {
            try
            {
                var events = await _service.GetEventByDate(date);

                return events switch
                {
                    null => Ok(new ItemListResponse<EventRequest>(ConstantMessage.Success, null)),
                    not null => Ok(new ItemListResponse<EventRequest>(ConstantMessage.Success, _mapper.Map<List<EventRequest>>(events)))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
