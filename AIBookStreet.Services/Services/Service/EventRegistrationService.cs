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
    public class EventRegistrationService(IUnitOfWork repository, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseService<EventRegistration>(mapper, repository, httpContextAccessor), IEventRegistrationService
    {
        private readonly IUnitOfWork _repository = repository;
        public async Task<(long, EventRegistration?)> AddAnEventRegistration(EventRegistrationModel model)
        {
            var existed = await _repository.EventRegistrationRepository.GetByEmail(model.EventId, model.RegistrantEmail);
            if (existed != null)
            {
                return (1, null); //da ton tai
            }
            var eventRegistrationModel = _mapper.Map<EventRegistration>(model);
            var setEventRegistrationModel = await SetBaseEntityToCreateFunc(eventRegistrationModel);
            var isSuccess = await _repository.EventRegistrationRepository.Add(setEventRegistrationModel);
            if (isSuccess)
            {
                return (2, setEventRegistrationModel);
            }
            return (3, null);
        }
        public async Task<(long, EventRegistration?)> UpdateAnEventRegistration(Guid? id, EventRegistrationModel model)
        {
            var existed = await _repository.EventRegistrationRepository.GetByID(id);
            if (existed == null)
            {
                return (1, null); //khong ton tai
            }
            if (existed.IsDeleted)
            {
                return (3, null);
            }
            existed.RegistrantName = model.RegistrantName;
            existed.RegistrantEmail = model.RegistrantEmail;
            existed.RegistrantPhoneNumber = model.RegistrantPhoneNumber;
            existed.RegistrantAgeRange = model.RegistrantAgeRange;
            existed.RegistrantGender = model.RegistrantGender;
            existed.RegistrantAddress = model.RegistrantAddress ?? existed.RegistrantAddress;
            existed.ReferenceSource = model.ReferenceSource ?? existed.ReferenceSource;
            existed.EventId = model.EventId ?? existed.EventId;
            existed = await SetBaseEntityToUpdateFunc(existed) ;
            return await _repository.EventRegistrationRepository.Update(existed) ? (2, existed) //update thanh cong
                                                                          : (3, null);       //update fail
        }
        public async Task<(long, EventRegistration?)> DeleteAnEventRegistration(Guid id)
        {
            var existed = await _repository.EventRegistrationRepository.GetByID(id);
            if (existed == null)
            {
                return (1, null); //khong ton tai
            }
            existed = await SetBaseEntityToUpdateFunc(existed);

            return await _repository.EventRegistrationRepository.Delete(existed) ? (2, existed) //delete thanh cong
                                                                          : (3, null);       //delete fail
        }
        public async Task<EventRegistration?> GetAnEventRegistrationById(Guid id)
        {
            return await _repository.EventRegistrationRepository.GetByID(id);
        }
        public async Task<List<EventRegistration>?> GetAllActiveEventRegistrations(Guid eventId)
        {
            var eventRegistrations = await _repository.EventRegistrationRepository.GetAll(eventId);

            return eventRegistrations.Count == 0 ? null : eventRegistrations;
        }
        public async Task<(List<object>, List<object>, List<object>, List<object>)> Test (Guid eventId)
        {
            return await _repository.EventRegistrationRepository.GetStatistic(eventId);
        }
    }
}
