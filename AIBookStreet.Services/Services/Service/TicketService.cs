using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.UnitOfWork.Interface;
using AIBookStreet.Services.Base;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Service
{
    public class TicketService(IUnitOfWork repository, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseService<Ticket>(mapper, repository, httpContextAccessor), ITicketService
    {
        private readonly IUnitOfWork _repository = repository;
        public async Task<(long, Ticket?, string?)> AddATicket(Guid? eventId)
        {
            try
            {
                if (eventId != null)
                {                    
                    var ticketCode = GenerateRandomString(6);
                    var ticketCodeExist = await _repository.TicketRepository.SearchTicketCode(eventId, ticketCode);
                    while (ticketCodeExist != null)
                    {
                        ticketCode = GenerateRandomString(6);
                        ticketCodeExist = await _repository.TicketRepository.SearchTicketCode(eventId, ticketCode);
                    }
                    var secretPasscode = GenerateRandomNumber(1000, 999999);
                    var secretPasscodeExist = await _repository.TicketRepository.SearchSecretPasscode(eventId, secretPasscode.ToString());
                    while (secretPasscodeExist != null)
                    {
                        secretPasscode = GenerateRandomNumber(1000, 999999);
                        secretPasscodeExist = await _repository.TicketRepository.SearchSecretPasscode(eventId, secretPasscode.ToString());
                    }
                    var model = new TicketModel
                    {
                        TicketCode = ticketCode.ToString(),
                        SecretPasscode = secretPasscode.ToString(),

                    };
                    var ticket = _mapper.Map<Ticket>(model);
                    var setTicket = await SetBaseEntityToCreateFunc(ticket);
                    var isSuccess = await _repository.TicketRepository.Add(setTicket);
                    if (isSuccess)
                    {
                        return (2, setTicket, null);
                    }
                }
                return (1, null, "Không tạo được vé");
            } catch
            {
                throw;
            }
        }
        public async Task<Ticket?> GetTicket(string email, string passcode)
        {
            try
            {
                return await _repository.TicketRepository.GetTicket(email, passcode);
            } catch
            {
                throw;
            }
        }
        public static string GenerateRandomString(int length)
        {
            try
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                Random random = new();
                return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            } catch
            {
                throw;
            }
        }
        public static int GenerateRandomNumber(int min, int max)
        {
            try
            {
                Random random = new();
                return random.Next(min, max + 1);
            } catch
            {
                throw;
            }
        }
        public async Task<Ticket?> GetTicketById (Guid? guid)
        {
            try
            {
                return await _repository.TicketRepository.GetByID(guid);
            } catch
            {
                throw;
            }
        }

        public async Task<(long, List<Ticket>?)> GetAllTicketOnEvent(Guid eventId)
        {
            try
            {
                var user = await GetUserInfo();
                var isOrganizer = false;
                if (user != null)
                {
                    foreach (var userRole in user.UserRoles)
                    {
                        if (userRole.Role.RoleName == "Organizer")
                        {
                            isOrganizer = true;
                        }
                    }
                }
                if (!isOrganizer)
                {
                    return (0, null);
                }
                var result = await _repository.TicketRepository.GetAllTicketOnEvent(eventId);
                return result.Count > 0 ? (2, result) : (1, null);
            } catch
            {
                throw;
            }
        }
    }
}
