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
    public class EventService(IUnitOfWork repository, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseService<Event>(mapper, repository, httpContextAccessor), IEventService
    {
        private readonly IUnitOfWork _repository = repository;
        public async Task<(long, Event?)> AddAnEvent(EventModel model)
        {
            var existed = await _repository.EventRepository.GetAllPagination(null, model.StartDate, model.EndDate, model.StreetId, 1, 1, null, true);
            if (existed.Item2 > 0 && existed.Item1 != null)
            {
                return (1, null); //da co su kien tren duong sach vao thoi gian nay
            }
            var evt = _mapper.Map<Event>(model);
            var setEvent = await SetBaseEntityToCreateFunc(evt);
            var isSuccess = await _repository.EventRepository.Add(setEvent);
            if (isSuccess)
            {
                return (2, setEvent);
            }
            return (3, null);
        }
        public async Task<(long, Event?)> UpdateAnEvent(Guid? id, EventModel model)
        {
            var existed = await _repository.EventRepository.GetByID(id);
            if (existed == null)
            {
                return (1, null); //khong ton tai
            }
            if (existed.IsDeleted)
            {
                return (3, null);
            }
            existed.EventName = model.EventName;
            existed.Description = model.Description ?? existed.Description;
            existed.StartDate = model.StartDate ?? existed.StartDate;
            existed.EndDate = model.EndDate ?? existed.EndDate;
            existed.StreetId = model.StreetId ?? existed.StreetId;

            existed = await SetBaseEntityToUpdateFunc(existed);
            return await _repository.EventRepository.Update(existed) ? (2, existed) //update thanh cong
                                                                          : (3, null);       //update fail
        }
        public async Task<(long, Event?)> DeleteAnEvent(Guid id)
        {
            var existed = await _repository.EventRepository.GetByID(id);
            if (existed == null)
            {
                return (1, null); //khong ton tai
            }
            existed = await SetBaseEntityToUpdateFunc(existed);

            return await _repository.EventRepository.Delete(existed) ? (2, existed) //delete thanh cong
                                                                          : (3, null);       //delete fail
        }
        public async Task<Event?> GetAnEventById(Guid id)
        {
            return await _repository.EventRepository.GetByID(id);
        }
        public async Task<(List<Event>?, long)> GetAllEventsPagination(string? key, DateTime? start, DateTime? end, Guid? streetID, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var user = await GetUserInfo();
            var isAdmin = false;
            if (user != null)
            {
                foreach (var userRole in user.UserRoles)
                {
                    if (userRole.Role.RoleName == "Quản trị viên")
                    {
                        isAdmin = true;
                    }
                }
            }
            var events = isAdmin ? await _repository.EventRepository.GetAllPaginationForAdmin(key, start, end, streetID, pageNumber, pageSize, sortField, desc)
                                                   : await _repository.EventRepository.GetAllPagination(key, start, end, streetID, pageNumber, pageSize, sortField, desc);
            return events.Item1.Count > 0 ? (events.Item1, events.Item2) : (null, 0);
        }
        public async Task<List<Event>?> GetEventComing(int number)
        {
            return await _repository.EventRepository.GetEventsComing(number);
        }
        public async Task<List<DateOnly>?> GetEventDatesInMonth(int month)
        {
            return await _repository.EventRepository.GetDatesInMonth(month);
        }
        public async Task<List<Event>?> GetEventByDate(DateTime date)
        {
            return await _repository.EventRepository.GetByDate(date);
        }
    }
}
