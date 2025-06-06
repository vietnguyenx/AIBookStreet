﻿using AIBookStreet.API.RequestModel;
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
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NuGet.Protocol.Core.Types;
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
                if (model.EventDates != null && model.StartTimes != null && model.EndTimes != null 
                    && model.EventDates.Count == model.StartTimes.Count 
                    && model.EventDates.Count == model.EndTimes.Count 
                    && model.StartTimes.Count == model.EndTimes.Count)
                {
                    var schedules = new List<EventScheduleModel>();
                    for(int i = 0;i< model.StartTimes.Count; i++)
                    {
                        schedules.Add(new EventScheduleModel
                        {
                            EventDate = model.EventDates[i],
                            StartTime = model.StartTimes[i],
                            EndTime = model.EndTimes[i]
                        });
                    }
                    var result = await _service.AddAnEvent(model, schedules);
                    return result.Item1 == 1 ? BadRequest(new BaseResponse(false, result.Item3 ?? ""))
                         : result.Item1 == 2 ? Ok(new ItemResponse<EventRequest>("Đã thêm", _mapper.Map<EventRequest>(result.Item2)))
                         : result.Item1 == 4 ? BadRequest(result.Item3)
                         : BadRequest(new BaseResponse(false, result.Item3 ?? ""));
                }
                return BadRequest(new BaseResponse(false, "Vui lòng điền giờ bắt đầu và giờ kết thúc tương ứng cho từng ngày diễn ra sự kiện"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPut("process-request/{id}")]
        public async Task<IActionResult> UpdateAnEvent([FromRoute] Guid id, [FromForm]ProcesingEventModel model)
        {
            try
            {
                var result = await _service.ProcessEvent(id, model);

                return result.Item1 switch
                {
                    1 => NotFound(new BaseResponse(false, "Không tồn tại!!!")),
                    2 => Ok(new ItemResponse<EventRequest>("Đã cập nhật thông tin!", _mapper.Map<EventRequest>(result.Item2))),
                    4 => BadRequest(result.Item3),
                    _ => BadRequest(new BaseResponse(false, result.Item3))
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
                    1 => NotFound(new BaseResponse(false, "Không tồn tại!!!")),
                    2 => Ok(new ItemResponse<EventRequest>("Đã xóa thành công!", _mapper.Map<EventRequest>(result.Item2))),
                    4 => BadRequest(result.Item3),
                    _ => BadRequest(new BaseResponse(false, result.Item3 ?? ""))
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
                eventInfor.StartDate = result.Item1.EventSchedules?.OrderBy(e => e.EventDate)?.FirstOrDefault()?.EventDate.ToString("yyyy-MM-dd");
                eventInfor.EndDate = result.Item1.EventSchedules?.OrderByDescending(e => e.EventDate)?.FirstOrDefault()?.EventDate.ToString("yyyy-MM-dd");
                eventInfor.TotalRegistrations = result.Item2;
                return Ok(new ItemResponse<EventRequest>(ConstantMessage.Success, eventInfor));

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

                if (events != null)
                {
                    var resp = new List<EventRequest>();
                    foreach (var evt in events)
                    {
                        var evtCovert = _mapper.Map<EventRequest>(evt);
                        evtCovert.StartDate = evt.EventSchedules?.OrderBy(e => e.EventDate)?.FirstOrDefault()?.EventDate.ToString("yyyy-MM-dd");
                        evtCovert.EndDate = evt.EventSchedules?.OrderByDescending(e => e.EventDate)?.FirstOrDefault()?.EventDate.ToString("yyyy-MM-dd");
                        resp.Add(evtCovert);
                    }
                    
                    return Ok(new ItemListResponse<EventRequest>(ConstantMessage.Success, resp));
                }
                return Ok(new ItemListResponse<EventRequest>(ConstantMessage.Success, null));
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

                if (events.Item2 == 0)
                {
                    return Ok(new PaginatedListResponse<EventRequest>(ConstantMessage.Success, null));
                }
                var resp = new List<EventRequest>();
                if (events.Item1 != null)
                {
                    foreach (var evt in events.Item1)
                    {
                        var evtCovert = _mapper.Map<EventRequest>(evt);
                        evtCovert.StartDate = evt.EventSchedules?.OrderBy(e => e.EventDate)?.FirstOrDefault()?.EventDate.ToString("yyyy-MM-dd");
                        evtCovert.EndDate = evt.EventSchedules?.OrderByDescending(e => e.EventDate)?.FirstOrDefault()?.EventDate.ToString("yyyy-MM-dd");
                        resp.Add(evtCovert);
                    }
                }

                return Ok(new PaginatedListResponse<EventRequest>(ConstantMessage.Success, resp, events.Item2, request != null ? request.PageNumber : 1, request != null ? request.PageSize : 10, request != null ? request.SortField : "CreatedDate", request != null && request.SortOrder != -1 ? 1 : -1));
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

                if (events != null)
                {
                    var resp = new List<EventRequest>();
                    foreach (var evt in events)
                    {
                        var evtCovert = _mapper.Map<EventRequest>(evt);
                        evtCovert.StartDate = evt.EventSchedules?.OrderBy(e => e.EventDate)?.FirstOrDefault()?.EventDate.ToString("yyyy-MM-dd");
                        evtCovert.EndDate = evt.EventSchedules?.OrderByDescending(e => e.EventDate)?.FirstOrDefault()?.EventDate.ToString("yyyy-MM-dd");
                        resp.Add(evtCovert);
                    }

                    return Ok(new ItemListResponse<EventRequest>(ConstantMessage.Success, resp));
                }
                return Ok(new ItemListResponse<EventRequest>(ConstantMessage.Success, null));
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

                if (events != null)
                {
                    var resp = new List<EventRequest>();
                    foreach (var evt in events)
                    {
                        var evtCovert = _mapper.Map<EventRequest>(evt);
                        evtCovert.StartDate = evt.EventSchedules?.OrderBy(e => e.EventDate)?.FirstOrDefault()?.EventDate.ToString("yyyy-MM-dd");
                        evtCovert.EndDate = evt.EventSchedules?.OrderByDescending(e => e.EventDate)?.FirstOrDefault()?.EventDate.ToString("yyyy-MM-dd");
                        resp.Add(evtCovert);
                    }

                    return Ok(new ItemListResponse<EventRequest>(ConstantMessage.Success, resp));
                }
                return Ok(new ItemListResponse<EventRequest>(ConstantMessage.Success, null));
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
        [Authorize]
        [HttpPost("events-in-date-for-checkin")]
        public async Task<IActionResult> GetAllEventsPaginationForStaff(PaginatedRequest<DateEventSearchRequest> request)
        {
            try
            {
                var events = await _service.GetEventsForCheckin(request.Result?.Date, request.Result?.EventName, request.PageNumber, request.PageSize, request.SortField, request.SortOrder == -1);

                if (events.Item2 == 0)
                {
                    return Ok(new PaginatedListResponse<EventRequest>(ConstantMessage.Success, null));
                }
                var resp = new List<EventRequest>();
                if (events.Item1 != null)
                {
                    foreach (var evt in events.Item1)
                    {
                        var evtCovert = _mapper.Map<EventRequest>(evt);
                        var date = request.Result?.Date != null ? request.Result?.Date : DateTime.Now;
                        var evtSchedule = evt.EventSchedules?.Where(es => es.EventDate == DateOnly.FromDateTime(date.Value.Date)).FirstOrDefault();
                        evtCovert.StartDate = evtSchedule?.EventDate.ToString("yyyy-MM-dd") + " " + evtSchedule?.StartTime.ToString("HH:mm:ss");
                        evtCovert.EndDate = evtSchedule?.EventDate.ToString("yyyy-MM-dd") + " " + evtSchedule?.EndTime.ToString("HH:mm:ss"); ;
                        resp.Add(evtCovert);
                    }
                }

                return Ok(new PaginatedListResponse<EventRequest>(ConstantMessage.Success, resp, resp.Count, request != null ? request.PageNumber : 1, request != null ? request.PageSize : 10, request != null ? request.SortField : "CreatedDate", request != null && request.SortOrder != -1 ? 1 : -1));
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [Authorize]
        [HttpPost("get-all-event-create-requests")]
        public async Task<IActionResult> GetRequest(PaginatedRequest? request)
        {
            try
            {
                var events = await _service.GetEventRequests(request?.PageNumber, request?.PageSize, request?.SortField, request?.SortOrder == -1);

                if (events.Item2 == 0)
                {
                    return Ok(new PaginatedListResponse<EventRequest>(ConstantMessage.Success, new List<EventRequest>(), events.Item2, request != null ? request.PageNumber : 1, request != null ? request.PageSize : 10, request != null ? request.SortField : "CreatedDate", request != null && request.SortOrder != -1 ? 1 : -1));
                }
                if (events.Item2 == 99)
                {
                    return BadRequest("Vui lòng đăng nhập với vai trò Quản trị viên");
                }
                var resp = new List<EventRequest>();
                if (events.Item1 != null)
                {
                    foreach (var evt in events.Item1)
                    {
                        var evtCovert = _mapper.Map<EventRequest>(evt);
                        evtCovert.StartDate = evt.EventSchedules?.OrderBy(e => e.EventDate)?.FirstOrDefault()?.EventDate.ToString("yyyy-MM-dd");
                        evtCovert.EndDate = evt.EventSchedules?.OrderByDescending(e => e.EventDate)?.FirstOrDefault()?.EventDate.ToString("yyyy-MM-dd");
                        resp.Add(evtCovert);
                    }
                }

                return Ok(new PaginatedListResponse<EventRequest>(ConstantMessage.Success, resp, events.Item2, request != null ? request.PageNumber : 1, request != null ? request.PageSize : 10, request != null ? request.SortField : "CreatedDate", request != null && request.SortOrder != -1 ? 1 : -1));
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [Authorize]
        [HttpPut("event-open-state/{id}")]
        public async Task<IActionResult> UpdateOpenStateForAnEvent([FromRoute] Guid id)
        {
            try
            {
                var result = await _service.OpenState(id);

                return result.Item1 switch
                {
                    1 => NotFound(new BaseResponse(false, "Không tồn tại!!!")),
                    2 => Ok(new ItemResponse<EventRequest>("Đã cập nhật thông tin!", _mapper.Map<EventRequest>(result.Item2))),
                    4 => BadRequest(result.Item3),
                    _ => BadRequest(new BaseResponse(false, result.Item3))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpGet("request-history/{id}")]
        public async Task<IActionResult> GetEventRequestHistoryById([FromRoute] Guid id)
        {
            try
            {
                var result = await _service.GetHistory(id);
                if (result.Item1 == 1)
                {
                    return NotFound(new ItemListResponse<List<EventRequest>>(ConstantMessage.NotFound));
                }
                var eventInfor = _mapper.Map<List<EventRequest>?>(result.Item2?.OrderBy(e => e.Version));
                return Ok(new ItemListResponse<EventRequest>(ConstantMessage.Success, eventInfor));

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
        [Authorize]
        [HttpPost("get-event-creations-history")]
        public async Task<IActionResult> GetCreationHistory(PaginatedRequest request)
        {
            try
            {
                var events = await _service.GetCreationHistory(request.PageNumber, request.PageSize);
                
                if (events.Item1 == 0)
                {
                    return BadRequest(events.Item3);
                }
                var response = new List<EventRequest>();
                if (events.Item2 != null)
                {
                    foreach (var evt in events.Item2)
                    {
                        var evtCovert = _mapper.Map<EventRequest>(evt);
                        evtCovert.StartDate = evt.EventSchedules?.OrderBy(e => e.EventDate)?.FirstOrDefault()?.EventDate.ToString("yyyy-MM-dd");
                        evtCovert.EndDate = evt.EventSchedules?.OrderByDescending(e => e.EventDate)?.FirstOrDefault()?.EventDate.ToString("yyyy-MM-dd");
                        response.Add(evtCovert);
                    }
                }

                return Ok(new PaginatedListResponse<EventRequest>(ConstantMessage.Success, response, events.Item2 != null ?events.Item2.Count : 0, request != null ? request.PageNumber : 1, request != null ? request.PageSize : 10, request != null ? request.SortField : "CreatedDate", request != null && request.SortOrder != -1 ? 1 : -1));
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
    }
}
