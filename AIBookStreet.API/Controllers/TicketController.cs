using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.API.Tool.Constant;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Services.Interface;
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
    }
}
