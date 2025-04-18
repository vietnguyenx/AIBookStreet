using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.UnitOfWork.Interface;
using AIBookStreet.Services.Base;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Service
{
    public class TicketService(IUnitOfWork repository, IMapper mapper) : BaseService<Ticket>(mapper, repository), ITicketService
    {
        private readonly IUnitOfWork _repository = repository;
        public async Task<Ticket?> AddATicket(TicketModel model)
        {
            try
            {
                var ticket = _mapper.Map<Ticket>(model);
                var setTicket = await SetBaseEntityToCreateFunc(ticket);
                var isSuccess = await _repository.TickRepository.Add(setTicket);
                if (isSuccess)
                {
                    return setTicket;
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Ticket?> GetTicket(string email, string passcode)
        {
            return await _repository.TickRepository.GetTicket(email, passcode);
        }
    }
}
