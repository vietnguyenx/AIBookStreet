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
    [Route("api/events")]
    [ApiController]
    public class EventController(IEventService service, IMapper mapper) : ControllerBase
    {
        private readonly IEventService _service = service;
        private readonly IMapper _mapper = mapper;

        [Authorize]
        [HttpPost("")]
        public async Task<IActionResult> AddAnEvent([FromForm]EventModel model)
        {
            try
            {
                var result = await _service.AddAnEvent(model);
                return result.Item1 == 1 ? BadRequest(new BaseResponse(false, "Đã có sự kiện trong thời gian trên khu vực này")) 
                     : result.Item1 == 2 ? Ok(new ItemResponse<EventRequest>("Đã thêm", _mapper.Map<EventRequest>(result.Item2)))
                     :                     BadRequest(new BaseResponse(false, "Đã xảy ra lỗi"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAnEvent([FromRoute] Guid id, [FromForm]EventModel model)
        {
            try
            {
                var result = await _service.UpdateAnEvent(id, model);

                return result.Item1 switch
                {
                    1 => BadRequest(new BaseResponse(false, "Không tồn tại!!!")),
                    2 => Ok(new ItemResponse<EventRequest>("Đã cập nhật thông tin!", _mapper.Map<EventRequest>(result.Item2))),
                    _ => BadRequest(new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại"))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> DeleteAnEvent([FromRoute]Guid id)
        {
            try
            {
                var result = await _service.DeleteAnEvent(id);

                return result.Item1 switch
                {
                    1 => BadRequest(new BaseResponse(false, "Không tồn tại!!!")),
                    2 => Ok(new ItemResponse<EventRequest>("Đã xóa thành công!", _mapper.Map<EventRequest>(result.Item2))),
                    _ => BadRequest(new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại!!!"))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAnEventById([FromRoute] Guid id)
        {
            try
            {
                var result = await _service.GetAnEventById(id);
                if (result.Item1 == null)
                {
                    return BadRequest(new ItemResponse<EventRequest>(ConstantMessage.NotFound));
                }
                var eventInfor = _mapper.Map<EventRequest>(result.Item1);
                eventInfor.AgeChart = result.Item2;
                eventInfor.GenderChart = result.Item3;
                eventInfor.ReferenceChart = result.Item4;
                eventInfor.AddressChart = result.Item5;
                eventInfor.AttendedChart = result.Item6;
                eventInfor.TotalRegistrations = result.Item7;
                return Ok(new ItemResponse<object>(ConstantMessage.Success, eventInfor));

                //return result.Item1 switch
                //{
                //    null => Ok(new ItemResponse<EventRequest>(ConstantMessage.NotFound)),
                //    not null => Ok(new ItemResponse<object>(ConstantMessage.Success, new
                //    {
                //        eventInfor = _mapper.Map<EventRequest>(result.Item1),
                //        ageChart = result.Item2,
                //        genderChart = result.Item3,
                //        referenceChart = result.Item4,
                //        addressChart = result.Item5
                //    }))
                //};
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [AllowAnonymous]
        [HttpGet("events-coming")]
        public async Task<IActionResult> GetEventsComing(int number, bool? allowAds)
        {
            try
            {
                var events = await _service.GetEventComing(number, allowAds);

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
        [HttpPost("search/paginated")]
        public async Task<IActionResult> GetAllEventsPagination(PaginatedRequest<EventSearchRequest> request)
        {
            try
            {
                var events = await _service.GetAllEventsPagination(request != null && request.Result != null ? request.Result.Key : null, request != null && request.Result != null ? request.Result.AllowAds : null, request != null && request.Result != null ? request.Result.StartDate : null, request != null && request.Result != null ? request.Result.EndDate : null, request != null && request.Result != null ? request.Result.ZoneId : null, request != null ? request.PageNumber : 1, request != null ? request.PageSize : 10, request != null ? request.SortField : "CreatedDate", request != null && request.SortOrder == -1);

                return events.Item2 switch
                {
                    0 => Ok(new PaginatedListResponse<EventRequest>(ConstantMessage.Success, null)),
                    _ => Ok(new PaginatedListResponse<EventRequest>(ConstantMessage.Success, _mapper.Map<List<EventRequest>>(events.Item1), events.Item2, request != null ? request.PageNumber : 1, request != null ? request.PageSize : 10, request != null ? request.SortField : "CreatedDate", request != null && request.SortOrder != -1 ? 1 : -1))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [AllowAnonymous]
        [HttpGet("event-dates-in-month")]
        public async Task<IActionResult> GetEventDatesInMonth(int? month)
        {
            try
            {
                if (month < 1 ||  month > 12)
                {
                    return Ok("Tháng phải trong khoảng 1 - 12 !!!");
                }
                var dates = await _service.GetEventDatesInMonth(month);

                return dates switch {
                    null => Ok(new ItemListResponse<DateModel>(ConstantMessage.Success, null)),
                    not null => Ok(new ItemListResponse<DateModel>(ConstantMessage.Success, dates)) 
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpGet("events-in-date")]
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
        [AllowAnonymous]
        [HttpGet("random")]
        public async Task<IActionResult> GetRandom(int number)
        {
            try
            {
                var events = await _service.GetRandom(number);

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
        [HttpGet("statistic/total")]
        public async Task<IActionResult> GetTotal(int month)
        {
            try
            {
                var events = await _service.GetNumberEventInMonth(month);

                return Ok(events);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
