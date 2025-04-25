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
        public async Task<IActionResult> AddAnEventRegistration(EventRegistrationModel model)
        {
            try
            {
                var result = await _service.AddAnEventRegistration(model);
                if (result.Item1 == 2)
                {
                    //await _service.SendEmai(result.Item2);
                    var ticket = await _ticketService.AddATicket(result.Item2.Id);
                    if (ticket.Item1 == 1)
                    {
                        return BadRequest(new BaseResponse(false, ticket.Item3));
                    }
                    await _service.SendEmai(ticket.Item2);
                    return Ok(new ItemResponse<object>("Đã thêm!", new
                    {
                        id = ticket.Item2?.Id,
                        ticketCode = ticket.Item2?.TicketCode,
                        eventId = ticket.Item2?.EventRegistration?.EventId,
                        registrationId = ticket.Item2?.RegistrationId,
                        attendeeName = ticket.Item2?.EventRegistration?.RegistrantName,
                        attendeeEmail = ticket.Item2?.EventRegistration?.RegistrantEmail,
                        attendeePhone = ticket.Item2?.EventRegistration?.RegistrantPhoneNumber,
                        attendeeAddress = ticket.Item2?.EventRegistration?.RegistrantAddress,
                        eventName = ticket.Item2?.EventRegistration?.Event?.EventName,
                        eventStartDate = ticket.Item2?.EventRegistration?.Event?.StartDate,
                        eventEndDate = ticket.Item2?.EventRegistration?.Event?.EndDate,
                        zoneId = ticket.Item2?.EventRegistration?.Event?.ZoneId,
                        zoneName = ticket.Item2?.EventRegistration?.Event?.Zone?.ZoneName,
                        latitude = ticket.Item2?.EventRegistration?.Event?.Zone?.Latitude,
                        longitude = ticket.Item2?.EventRegistration?.Event?.Zone?.Longitude,
                        issuedAt = ticket.Item2?.CreatedDate
                    }));

                }
                return result.Item1 switch
                {
                    1 => BadRequest(new BaseResponse(false, result.Item3)),
                    _ => Ok(new BaseResponse(false, result.Item3))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpPut("check-attendend")]
        public async Task<IActionResult> UpdateAnEventRegistration(CheckAttendModel model)
        {
            try
            {
                var result = await _service.CheckAttend(model);

                return result.Item1 switch
                {
                    1 => Ok(new BaseResponse(false, "Không tồn tại!!!")),
                    2 => Ok(new BaseResponse(true, "Đã cập nhật trạng thái!")),
                    _ => Ok(new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại"))
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
        [AllowAnonymous]
        [HttpGet("get-all/{eventId}")]
        public async Task<IActionResult> GetAllEventRegistrations([FromRoute]Guid eventId)
        {
            try
            {
                var eventRegistrations = await _service.GetAllActiveEventRegistrations(eventId);

                return eventRegistrations switch
                {
                    null => Ok(new ItemListResponse<EventRegistrationRequest>(ConstantMessage.Success, null)),
                    not null => Ok(new ItemListResponse<EventRegistrationRequest>(ConstantMessage.Success, _mapper.Map<List<EventRegistrationRequest>>(eventRegistrations)))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpGet("statistic/{eventId}")]
        public async Task<IActionResult> Test([FromRoute] Guid eventId)
        {
            var result = await _service.Test(eventId);
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
                attendedChart = result.Item5,
                totalRegistrations = result.Item6,
                participation = result.Item7,
                participationRate = (result.Item7 * 100) / result.Item6 + "%"
            });
        }
    }
}
