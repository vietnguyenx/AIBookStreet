using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.API.Tool.Constant;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Security.Policy;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AIBookStreet.API.Controllers
{
    [Route("api/event-registrations")]
    [ApiController]
    public class EventRegistrationController(IEventRegistrationService service, IMapper mapper, ITicketService ticketService) : ControllerBase
    {
        private readonly IEventRegistrationService _service = service;
        private readonly ITicketService _ticketService = ticketService;
        private readonly IMapper _mapper = mapper;
        [AllowAnonymous]
        [HttpPost("")]
        public async Task<IActionResult> AddAnEventRegistration([FromQuery]EventRegistrationModel model)
        {
            try
            {
                var result = await _service.AddAnEventRegistration(model);
                if (result.Item1 == 2)
                {
                    if (result.Item2?.FirstOrDefault()?.TicketId != null)
                    {
                        //await _service.SendEmai(ticket.Item2);
                        var eventRegistrationQueue = HttpContext.RequestServices.GetRequiredService<IEventRegistrationQueueService>();
                        eventRegistrationQueue.Enqueue(result.Item2?.FirstOrDefault()?.TicketId);
                    }
                    var dates = new List<object>();
                    if (result.Item2 != null)
                    {
                        foreach (var item in result.Item2)
                        {
                            var schedule = result.Item2?.FirstOrDefault()?.Event?.EventSchedules?.Where(e => e.EventDate == item.DateToAttend).FirstOrDefault();
                            dates.Add(
                                new
                                {
                                    date = item.DateToAttend.ToString("yyyy-MM-dd"),
                                    startTime = schedule?.StartTime.ToString("HH:mm:ss"),
                                    endTime = schedule?.EndTime.ToString("HH:mm:ss")
                                });
                        }
                    }
                    return Ok(new ItemResponse<object>(result.Item3, new
                    {
                        id = result.Item2?.FirstOrDefault() != null ? result.Item2?.FirstOrDefault()?.TicketId : null,
                        ticketCode = result.Item2?.FirstOrDefault() != null ? result.Item2?.FirstOrDefault()?.Ticket?.TicketCode.ToString() : null,
                        eventId = result.Item2?.FirstOrDefault() != null ? result.Item2?.FirstOrDefault()?.EventId : null,
                        registrationId = result.Item2?.FirstOrDefault() != null ? result.Item2?.FirstOrDefault()?.Id : null,
                        attendeeName = result.Item2?.FirstOrDefault() != null ? result.Item2?.FirstOrDefault()?.RegistrantName : null,
                        attendeeEmail = result.Item2?.FirstOrDefault() != null ? result.Item2?.FirstOrDefault()?.RegistrantEmail : null,
                        attendeePhone = result.Item2?.FirstOrDefault() != null ? result.Item2?.FirstOrDefault()?.RegistrantPhoneNumber : null,
                        attendeeAddress = result.Item2?.FirstOrDefault() != null ? result.Item2?.FirstOrDefault()?.RegistrantAddress : null,
                        eventName = (result.Item2?.FirstOrDefault() != null && result.Item2?.FirstOrDefault()?.Event != null) ? result.Item2?.FirstOrDefault()?.Event?.EventName : null,
                        eventStartDate = (result.Item2?.FirstOrDefault() != null && result.Item2?.FirstOrDefault()?.Event != null && result.Item2?.FirstOrDefault()?.Event?.EventSchedules != null) ? result.Item2?.FirstOrDefault()?.Event?.EventSchedules?.OrderBy(e => e.EventDate).FirstOrDefault()?.EventDate.ToString("yyyy-MM-dd") : null,
                        eventEndDate = (result.Item2?.FirstOrDefault() != null && result.Item2?.FirstOrDefault()?.Event != null && result.Item2?.FirstOrDefault()?.Event?.EventSchedules != null) ? result.Item2?.FirstOrDefault()?.Event?.EventSchedules?.OrderByDescending(e => e.EventDate).FirstOrDefault()?.EventDate.ToString("yyyy-MM-dd") : null,
                        registeredDates = dates,
                        zoneId = (result.Item2?.FirstOrDefault() != null && result.Item2?.FirstOrDefault()?.Event != null) ? result.Item2?.FirstOrDefault()?.Event?.ZoneId : null,
                        zoneName = (result.Item2?.FirstOrDefault() != null && result.Item2?.FirstOrDefault()?.Event != null && result.Item2?.FirstOrDefault()?.Event?.Zone != null) ? result.Item2?.FirstOrDefault()?.Event?.Zone?.ZoneName : null,
                        latitude = (result.Item2?.FirstOrDefault() != null && result.Item2?.FirstOrDefault()?.Event != null && result.Item2?.FirstOrDefault()?.Event?.Zone != null) ? result.Item2?.FirstOrDefault()?.Event?.Zone?.Latitude : null,
                        longitude = (result.Item2?.FirstOrDefault() != null && result.Item2?.FirstOrDefault()?.Event != null && result.Item2?.FirstOrDefault()?.Event?.Zone != null) ? result.Item2?.FirstOrDefault()?.Event?.Zone?.Longitude : null,
                        issuedAt = result.Item2?.FirstOrDefault() != null ? result.Item2?.FirstOrDefault()?.Ticket?.CreatedDate : null,
                    }));

                }
                return result.Item1 switch
                {
                    3 => BadRequest(new BaseResponse(false, result.Item3)),
                    _ => Ok(new BaseResponse(false, result.Item3))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPut("check-attendend")]
        public async Task<IActionResult> UpdateAnEventRegistration(List<CheckAttendModel> models)
        {
            try
            {
                var evtRegis = await _service.GetAnEventRegistrationById(models.First().Id);
                var result = await _service.CheckAttend(models, evtRegis?.Event);

                return result.Item1 switch
                {
                    0 => BadRequest(result.Item3),
                    _ => Ok(new BaseResponse(true, result.Item3))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[AllowAnonymous]
        //[HttpPatch("{id}")]
        //public async Task<IActionResult> DeleteAnEventRegistration([FromRoute] Guid id)
        //{
        //    try
        //    {
        //        var result = await _service.DeleteAnEventRegistration(id);

        //        return result.Item1 switch
        //        {
        //            1 => Ok(new BaseResponse(false, "Không tồn tại!!!")),
        //            2 => Ok(new BaseResponse(true, "Đã xóa thông tin!")),
        //            _ => Ok(new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại"))
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAnEventRegistrationById([FromRoute] Guid id)
        {
            try
            {
                var eventRegistration = await _service.GetAnEventRegistrationById(id);

                return eventRegistration switch
                {
                    null => BadRequest(new ItemResponse<EventRegistration>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<EventRegistration>(ConstantMessage.Success, eventRegistration))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [Authorize]
        [HttpGet("get-all/{eventId}")]
        public async Task<IActionResult> GetAllActiveEventRegistrationsInAnEvent([FromRoute]Guid eventId, string? searchKey, string? date)
        {
            try
            {
                var eventRegistrations = await _service.GetAllActiveEventRegistrationsInAnEvent(eventId, searchKey, date);

                return eventRegistrations.Item1 switch
                {
                    0 => BadRequest("Hãy đăng nhập với vai trò Người tổ chức sự kiện"),
                    1 => Ok(new ItemListResponse<EventRegistrationRequest>(ConstantMessage.Success, null)),
                    _ => Ok(new ItemListResponse<EventRegistrationRequest>(ConstantMessage.Success, _mapper.Map<List<EventRegistrationRequest>>(eventRegistrations.Item2)))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpGet("statistic/{eventId}")]
        public async Task<IActionResult> GetAnEventStatistics([FromRoute] Guid eventId, bool? isAttended, string? province, string? district, string? date)
        {
            try
            {
                var result = await _service.GetAnEventStatistics(eventId, isAttended, province, district, date);
                if (result.Item6 == 0)
                {
                    return BadRequest("Chưa có thông tin đăng ký của sự kiện này");
                }
                return Ok(new
                    {
                        success = true,
                        ageChart = result.Item1,
                        genderChart = result.Item2,
                        referenceChart = result.Item3,
                        addressChart = result.Item4,
                        attendedBeforeChart = result.Item5,
                        totalRegistrations = result.Item6,
                        participation = result.Item7,
                        participationRate = (result.Item7 * 100) / result.Item6 + "%"
                    });
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpPost("resend-mail")]
        public async Task<IActionResult> ResendMail(Guid registrtionId)
        {
            try
            {
                var eventRegistration = await _service.GetAnEventRegistrationById(registrtionId);
                if (eventRegistration != null)
                {
                    if (eventRegistration?.RegistrantEmail != null && eventRegistration.TicketId != null)
                    {
                        var emailQueue = HttpContext.RequestServices.GetRequiredService<IEventRegistrationQueueService>();
                        emailQueue.Enqueue(eventRegistration.TicketId);
                    }
                    var dates = new List<object>();
                    if (eventRegistration != null)
                    {
                            var schedule = eventRegistration?.Event?.EventSchedules?.Where(e => e.EventDate == eventRegistration.DateToAttend).FirstOrDefault();
                            dates.Add(
                                new
                                {
                                    date = eventRegistration?.DateToAttend.ToString("yyyy-MM-dd"),
                                    startTime = schedule?.StartTime.ToString("HH:mm:ss"),
                                    endTime = schedule?.EndTime.ToString("HH:mm:ss")
                                });
                        
                    }
                    return Ok(new ItemResponse<object>("Đã thêm!", new
                    {
                        id = eventRegistration?.TicketId,
                        ticketCode = eventRegistration?.Ticket?.TicketCode,
                        eventId = eventRegistration?.EventId,
                        registrationId = eventRegistration?.Id,
                        attendeeName = eventRegistration?.RegistrantName,
                        attendeeEmail = eventRegistration?.RegistrantEmail,
                        attendeePhone = eventRegistration?.RegistrantPhoneNumber,
                        attendeeAddress = eventRegistration?.RegistrantAddress,
                        eventName = eventRegistration?.Event?.EventName,
                        eventStartDate = eventRegistration?.Event?.EventSchedules?.OrderBy(e => e.EventDate).FirstOrDefault()?.EventDate,
                        eventEndDate = eventRegistration?.Event?.EventSchedules?.OrderByDescending(e => e.EventDate).FirstOrDefault()?.EventDate,
                        registeredDates = dates,
                        zoneId = eventRegistration?.Event?.ZoneId,
                        zoneName = eventRegistration?.Event?.Zone?.ZoneName,
                        latitude = eventRegistration?.Event?.Zone?.Latitude,
                        longitude = eventRegistration?.Event?.Zone?.Longitude,
                        issuedAt = eventRegistration?.Ticket?.CreatedDate,
                    }));
                }
                return BadRequest("Không thể gửi email, không tìm thấy email đăng ký");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[Authorize]
        //[HttpPut("check-list-attended")]
        //public async Task<IActionResult> UpdateEventRegistrations(List<EventRegistrationRequest> list)
        //{
        //    try
        //    {
        //        var models = _mapper.Map<List<CheckAttendModel>>(list);
        //        var result = await _service.CheckListAttend(models);

        //        return result.Item1 switch
        //        {
        //            0 => BadRequest("Hãy đăng nhập với vai trò quản trị viên"),
        //            1 => BadRequest(new BaseResponse(false, "Không tồn tại!!!")),
        //            2 => Ok(new BaseResponse(true, "Đã cập nhật trạng thái!")),
        //            4 => BadRequest("Đã quá hạn điểm danh"),
        //            5 => BadRequest("Danh sách trống"),
        //            _ => BadRequest(new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại"))
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
        [AllowAnonymous]
        [HttpGet("export-statistic/{eventId}")]
        public IActionResult ExportAnEventStatistics([FromRoute] Guid eventId, string? email)
        {
            try
            {
                var model = new ExportStatisticModel
                {
                    EventId = eventId,
                    Email = email
                };
                var exportEventStatisticQueue = HttpContext.RequestServices.GetRequiredService<IExportEventStatisticQueueService>();
                exportEventStatisticQueue.Enqueue(model);
                return Ok(new BaseResponse(true, "File số liệu sẽ được gửi tới email của bạn"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
