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

namespace AIBookStreet.API.Controllers
{
    [Route("api/event-registrations")]
    [ApiController]
    public class EventRegistrationController(IEventRegistrationService service, IMapper mapper) : ControllerBase
    {
        private readonly IEventRegistrationService _service = service;
        private readonly IMapper _mapper = mapper;
        [AllowAnonymous]
        [HttpPost("")]
        public async Task<IActionResult> AddAnEventRegistration(EventRegistrationModel model)
        {
            try
            {
                var result = await _service.AddAnEventRegistration(model);
                return result.Item1 switch
                {
                    1 => Ok(new BaseResponse(false, "Đã tồn tại!!!")),
                    2 => Ok(new ItemResponse<object>("Đã thêm!", new
                    {
                        id = result.Item2.Id,
                        ticketCode = result.Item2.TicketCode,
                        eventId = result.Item2.EventRegistration.EventId,
                        registrationId = result.Item2.RegistrationId,
                        attendeeName = result.Item2.EventRegistration.RegistrantName,
                        attendeeEmail = result.Item2.EventRegistration.RegistrantEmail,
                        attendeePhone = result.Item2.EventRegistration.RegistrantPhoneNumber,
                        attendeeAddress = result.Item2.EventRegistration.RegistrantAddress,
                        eventName = result.Item2.EventRegistration.Event.EventName,
                        eventStartDate = result.Item2.EventRegistration.Event.StartDate,
                        eventEndDate = result.Item2.EventRegistration.Event.EndDate,
                        zoneId = result.Item2.EventRegistration.Event.ZoneId,
                        zoneName = result.Item2.EventRegistration.Event.Zone.ZoneName,
                        latitude = result.Item2.EventRegistration.Event.Zone.Latitude,
                        longitude = result.Item2.EventRegistration.Event.Zone.Longitude,
                        issuedAt = result.Item2.CreatedDate
                    })),
                    _ => Ok(new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại"))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[AllowAnonymous]
        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateAnEventRegistration([FromRoute] Guid id, EventRegistrationModel model)
        //{
        //    try
        //    {
        //        var result = await _service.UpdateAnEventRegistration(id, model);

        //        return result.Item1 switch
        //        {
        //            1 => Ok(new BaseResponse(false, "Không tồn tại!!!")),
        //            2 => Ok(new BaseResponse(true, "Đã cập nhật thông tin!")),
        //            _ => Ok(new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại"))
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
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
                    null => Ok(new ItemResponse<EventRegistration>(ConstantMessage.NotFound)),
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
            return Ok(new
            {
                success = true,
                ageChart = result.Item1,
                genderChart = result.Item2,
                referenceChart = result.Item3,
                addressChart = result.Item4,
                attendedChart = result.Item5
            });
        }
    }
}
