using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.API.Tool.Constant;
using AIBookStreet.Repositories.Data.Entities;
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
        public async Task<IActionResult> GetAnEventRegistrationById([FromRoute]string email, string passcode)
        {
            try
            {
                var ticket = await _service.GetTicket(email, passcode);
                if (ticket == null)
                {
                    return BadRequest(new ItemResponse<Ticket>(ConstantMessage.NotFound));
                }

                return Ok(new ItemResponse<TicketRequest>(ConstantMessage.Success, _mapper.Map<TicketRequest>(ticket)));
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Create (Guid registrantId)
        {
            try
            {
                var ticket = await _service.AddATicket(registrantId);

                return ticket.Item2 switch
                {
                    null => BadRequest(new ItemResponse<Ticket>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<TicketRequest>(ConstantMessage.Success, _mapper.Map<TicketRequest>(ticket.Item2)))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAnEventResgisById([FromRoute]Guid id)
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
