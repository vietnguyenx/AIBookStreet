using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.API.Tool.Constant;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AIBookStreet.Services.Services.Service;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AIBookStreet.API.Controllers
{
    [Route("api/tickets")]
    [ApiController]
    public class TicketController(ITicketService service, IMapper mapper) : ControllerBase
    {
        private readonly ITicketService _service = service;
        private readonly IMapper _mapper = mapper;
        [AllowAnonymous]
        [HttpGet("{email}/{passcode}")]
        public async Task<IActionResult> GetATicketByEmailAndPasscode([FromRoute]string email, string passcode)
        {
            try
            {
                var ticket = await _service.GetTicket(email, passcode);
                if (ticket == null)
                {
                    return BadRequest(new ItemResponse<Ticket>(ConstantMessage.NotFound));
                }
                //var response = _mapper.Map<TicketResponse>(ticket);
                //response.EventRegistration = _mapper.Map<EventRegistrationResponse>(ticket.EventRegistrations?.FirstOrDefault());
                //response.EventRegistration.Event = _mapper.Map<EventResponse>(ticket.EventRegistrations?.FirstOrDefault()?.Event);
                //response.EventRegistration.Event.StartDate = ticket.EventRegistrations?.FirstOrDefault()?.Event?.EventSchedules?.OrderBy(es => es.EventDate).FirstOrDefault()?.EventDate.ToString("yyyy-MM-dd");
                //response.EventRegistration.Event.EndDate = ticket.EventRegistrations?.FirstOrDefault()?.Event?.EventSchedules?.OrderByDescending(es => es.EventDate).FirstOrDefault()?.EventDate.ToString("yyyy-MM-dd");

                //var dates = new List<object>();
                //if (ticket.EventRegistrations != null)
                //{
                //    foreach (var item in ticket.EventRegistrations)
                //    {
                //        dates.Add(new { date = item.DateToAttend.ToString("yyyy-MM-dd") });
                //    }
                //}
                //var dates = new List<object>();
                //if (ticket.EventRegistrations != null)
                //{
                //    foreach (var item in ticket.EventRegistrations)
                //    {
                //        var schedule = ticket.EventRegistrations?.FirstOrDefault()?.Event?.EventSchedules?.Where(e => e.EventDate == item.DateToAttend).FirstOrDefault();
                //        dates.Add(new { date = item.DateToAttend.ToString("yyyy-MM-dd") + " " + schedule?.StartTime.ToString("HH:mm") + "-" + schedule?.EndTime.ToString("HH:mm") });
                //    }
                //}

                var dates = new List<object>();
                if (ticket.EventRegistrations != null)
                {
                    foreach (var item in ticket.EventRegistrations)
                    {
                        var schedule = ticket.EventRegistrations?.FirstOrDefault()?.Event?.EventSchedules?.Where(e => e.EventDate == item.DateToAttend).FirstOrDefault();
                        dates.Add(
                            new
                            {
                                date = item.DateToAttend.ToString("yyyy-MM-dd"),
                                startTime = schedule?.StartTime.ToString("HH:mm:ss"),
                                endTime = schedule?.EndTime.ToString("HH:mm:ss")
                            });
                    }
                }
                var response = new
                {
                    id = ticket.EventRegistrations?.FirstOrDefault() != null ? ticket.EventRegistrations?.FirstOrDefault()?.TicketId : null,
                    ticketCode = ticket.EventRegistrations?.FirstOrDefault() != null ? ticket.EventRegistrations?.FirstOrDefault()?.Ticket?.TicketCode : null,
                    eventId = ticket.EventRegistrations?.FirstOrDefault() != null ? ticket.EventRegistrations?.FirstOrDefault()?.EventId : null,
                    registrationId = ticket.EventRegistrations?.FirstOrDefault() != null ? ticket.EventRegistrations?.FirstOrDefault()?.Id : null,
                    attendeeName = ticket.EventRegistrations?.FirstOrDefault() != null ? ticket.EventRegistrations?.FirstOrDefault()?.RegistrantName : null,
                    attendeeEmail = ticket.EventRegistrations?.FirstOrDefault() != null ? ticket.EventRegistrations?.FirstOrDefault()?.RegistrantEmail : null,
                    attendeePhone = ticket.EventRegistrations?.FirstOrDefault() != null ? ticket.EventRegistrations?.FirstOrDefault()?.RegistrantPhoneNumber : null,
                    attendeeAddress = ticket.EventRegistrations?.FirstOrDefault() != null ? ticket.EventRegistrations?.FirstOrDefault()?.RegistrantAddress : null,
                    eventName = (ticket.EventRegistrations?.FirstOrDefault() != null && ticket.EventRegistrations?.FirstOrDefault()?.Event != null) ? ticket.EventRegistrations?.FirstOrDefault()?.Event?.EventName : null,
                    eventStartDate = (ticket.EventRegistrations?.FirstOrDefault() != null && ticket.EventRegistrations?.FirstOrDefault()?.Event != null && ticket.EventRegistrations?.FirstOrDefault()?.Event?.EventSchedules != null) ? ticket.EventRegistrations?.FirstOrDefault()?.Event?.EventSchedules?.OrderBy(e => e.EventDate).FirstOrDefault()?.EventDate : null,
                    eventEndDate = (ticket.EventRegistrations?.FirstOrDefault() != null && ticket.EventRegistrations?.FirstOrDefault()?.Event != null && ticket.EventRegistrations?.FirstOrDefault()?.Event?.EventSchedules != null) ? ticket.EventRegistrations?.FirstOrDefault()?.Event?.EventSchedules?.OrderByDescending(e => e.EventDate).FirstOrDefault()?.EventDate : null,
                    registeredDates = dates,
                    zoneId = (ticket.EventRegistrations?.FirstOrDefault() != null && ticket.EventRegistrations?.FirstOrDefault()?.Event != null) ? ticket.EventRegistrations?.FirstOrDefault()?.Event?.ZoneId : null,
                    zoneName = (ticket.EventRegistrations?.FirstOrDefault() != null && ticket.EventRegistrations?.FirstOrDefault()?.Event != null && ticket.EventRegistrations?.FirstOrDefault()?.Event?.Zone != null) ? ticket.EventRegistrations?.FirstOrDefault()?.Event?.Zone?.ZoneName : null,
                    latitude = (ticket.EventRegistrations?.FirstOrDefault() != null && ticket.EventRegistrations?.FirstOrDefault()?.Event != null && ticket.EventRegistrations?.FirstOrDefault()?.Event?.Zone != null) ? ticket.EventRegistrations?.FirstOrDefault()?.Event?.Zone?.Latitude : null,
                    longitude = (ticket.EventRegistrations?.FirstOrDefault() != null && ticket.EventRegistrations?.FirstOrDefault()?.Event != null && ticket.EventRegistrations?.FirstOrDefault()?.Event?.Zone != null) ? ticket.EventRegistrations?.FirstOrDefault()?.Event?.Zone?.Longitude : null,
                    issuedAt = ticket.EventRegistrations?.FirstOrDefault() != null ? ticket.EventRegistrations?.FirstOrDefault()?.Ticket?.CreatedDate : null,
                };
                return Ok(new ItemResponse<object>(ConstantMessage.Success, response));
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        //[AllowAnonymous]
        //[HttpPost]
        //public async Task<IActionResult> Create (Guid registrantId)
        //{
        //    try
        //    {
        //        var ticket = await _service.AddATicket(registrantId);

        //        return ticket.Item2 switch
        //        {
        //            null => BadRequest(new ItemResponse<Ticket>(ConstantMessage.NotFound)),
        //            not null => Ok(new ItemResponse<TicketRequest>(ConstantMessage.Success, _mapper.Map<TicketRequest>(ticket.Item2)))
        //        };
        //    }
        //    catch (Exception ex)
        //    {

        //        return BadRequest(ex.Message);
        //    };
        //}
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetATicketById([FromRoute]Guid id)
            {
            try
            {
                var ticket = await _service.GetTicketById(id);

                return ticket switch
                {
                    null => BadRequest(new ItemResponse<Ticket>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<TicketRequest>(ConstantMessage.Success, _mapper.Map<TicketRequest>(ticket)))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [Authorize]
        [HttpGet("all-on-event/{eventId}")]
        public async Task<IActionResult> GetAll([FromRoute] Guid eventId)
        {
            try
            {
                var result = await _service.GetAllTicketOnEvent(eventId);
                //var resp = new List<object>();
                if (result.Item1 == 2)
                {                    
                    //foreach(var ticket in result.Item2)
                    //{
                    //    resp.Add(new
                    //    {
                    //        id = ticket.Id,
                    //        ticketCode = ticket.TicketCode,
                    //        secretPasscode = ticket.SecretPasscode,
                    //        eventId = ticket.EventRegistration?.EventId,
                    //        registrationId = ticket.RegistrationId,
                    //        attendeeName = ticket.EventRegistration?.RegistrantName,
                    //        attendeeEmail = ticket.EventRegistration?.RegistrantEmail,
                    //        attendeePhone = ticket.EventRegistration?.RegistrantPhoneNumber,
                    //        attendeeAddress = ticket.EventRegistration?.RegistrantAddress,
                    //        eventName = ticket.EventRegistration?.Event?.EventName,
                    //        eventStartDate = ticket.EventRegistration?.Event?.StartDate,
                    //        eventEndDate = ticket.EventRegistration?.Event?.EndDate,
                    //        zoneId = ticket.EventRegistration?.Event?.ZoneId,
                    //        zoneName = ticket.EventRegistration?.Event?.Zone?.ZoneName,
                    //        latitude = ticket.EventRegistration?.Event?.Zone?.Latitude,
                    //        longitude = ticket.EventRegistration?.Event?.Zone?.Longitude,
                    //        issuedAt = ticket.CreatedDate
                    //    });
                    //}
                    return Ok(new ItemListResponse<TicketRequest>(ConstantMessage.Success, _mapper.Map<List<TicketRequest>>(result.Item2)));

                }
                return result.Item1 switch
                {
                    0 => BadRequest(new BaseResponse(false, "Vui lòng đăng nhập với vai trò Nhân viên")),
                    _ => Ok(new ItemListResponse<object>(ConstantMessage.Success, null ))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
