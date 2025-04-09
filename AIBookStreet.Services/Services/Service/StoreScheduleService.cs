using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using AIBookStreet.Repositories.Repositories.UnitOfWork.Interface;
using AIBookStreet.Services.Base;
using AIBookStreet.Services.Common;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Service
{
    public class StoreScheduleService : BaseService<StoreSchedule>, IStoreScheduleService
    {
        private readonly IStoreScheduleRepository _storeScheduleRepository;

        public StoreScheduleService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(mapper, unitOfWork, httpContextAccessor)
        {
            _storeScheduleRepository = unitOfWork.StoreScheduleRepository;

        }

        public async Task<List<StoreScheduleModel>> GetAll()
        {
            var schedules = await _storeScheduleRepository.GetAll();
            if (!schedules.Any())
                return null;
            return _mapper.Map<List<StoreScheduleModel>>(schedules);
        }

        public async Task<StoreScheduleModel?> GetById(Guid id)
        {
            var schedule = await _storeScheduleRepository.GetById(id);
            if (schedule == null)
                return null;
            return _mapper.Map<StoreScheduleModel>(schedule);
        }

        public async Task<List<StoreScheduleModel>?> GetByStoreId(Guid storeId)
        {
            var schedules = await _storeScheduleRepository.GetByStoreId(storeId);
            if (!schedules.Any())
                return null;
            return _mapper.Map<List<StoreScheduleModel>>(schedules);
        }

        public async Task<StoreScheduleModel?> GetByStoreIdAndDayOfWeek(Guid storeId, DayOfWeek dayOfWeek)
        {
            var schedule = await _storeScheduleRepository.GetByStoreIdAndDayOfWeek(storeId, dayOfWeek);
            if (schedule == null)
                return null;
            return _mapper.Map<StoreScheduleModel>(schedule);
        }

        public async Task<StoreScheduleModel?> GetByStoreIdAndSpecialDate(Guid storeId, DateTime specialDate)
        {
            var schedule = await _storeScheduleRepository.GetByStoreIdAndSpecialDate(storeId, specialDate);
            if (schedule == null)
                return null;
            return _mapper.Map<StoreScheduleModel>(schedule);
        }

        public async Task<(StoreScheduleModel?, string)> Add(StoreScheduleModel model)
        {
            try
            {
                if (model == null)
                    return (null, ConstantMessage.Common.EmptyInfo);

                if (model.StoreId == Guid.Empty)
                    return (null, ConstantMessage.EmptyId);

                //Check if special date schedule da co chua
                var existingSchedule = model.SpecialDate.HasValue
                    ? await _storeScheduleRepository.GetByStoreIdAndSpecialDate(model.StoreId, model.SpecialDate.Value)
                    : await _storeScheduleRepository.GetByStoreIdAndDayOfWeek(model.StoreId, model.DayOfWeek);

                if (existingSchedule != null)
                    return (null, "A calendar already exists for this day");

                // Validate time format
                if (!TimeSpan.TryParse(model.OpenTime, out _) || !TimeSpan.TryParse(model.CloseTime, out _))
                {
                    return (null, "Invalid time format. Please use HH:mm:ss format");
                }

                var mappedSchedule = _mapper.Map<StoreSchedule>(model);
                var newSchedule = await SetBaseEntityToCreateFunc(mappedSchedule);

                var result = await _storeScheduleRepository.Add(newSchedule);
                if (!result)
                    return (null, ConstantMessage.Common.AddFail);

                return (_mapper.Map<StoreScheduleModel>(newSchedule), ConstantMessage.Common.AddSuccess);
            }
            catch (Exception ex)
            {
                return (null, $"Error while adding store schedule: {ex.Message}");
            }
        }

        public async Task<(StoreScheduleModel?, string)> Update(StoreScheduleModel model)
        {
            try
            {
                if (model == null)
                    return (null, ConstantMessage.Common.EmptyInfo);

                if (model.Id == Guid.Empty)
                    return (null, ConstantMessage.EmptyId);

                var existingSchedule = await _storeScheduleRepository.GetById(model.Id);
                if (existingSchedule == null)
                    return (null, ConstantMessage.Common.NotFoundForUpdate);

                // Validate time format
                if (!TimeSpan.TryParse(model.OpenTime, out _) || !TimeSpan.TryParse(model.CloseTime, out _))
                {
                    return (null, "Invalid time format. Please use HH:mm:ss format");
                }

                _mapper.Map(model, existingSchedule);
                var updatedSchedule = await SetBaseEntityToUpdateFunc(existingSchedule);

                var result = await _storeScheduleRepository.Update(updatedSchedule);
                if (!result)
                    return (null, ConstantMessage.Common.UpdateFail);

                return (_mapper.Map<StoreScheduleModel>(updatedSchedule), ConstantMessage.Common.UpdateSuccess);
            }
            catch (Exception ex)
            {
                return (null, $"Error while updating store schedule: {ex.Message}");
            }
        }

        public async Task<(StoreScheduleModel?, string)> Delete(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return (null, ConstantMessage.EmptyId);

                var existingSchedule = await _storeScheduleRepository.GetById(id);
                if (existingSchedule == null)
                    return (null, ConstantMessage.Common.NotFoundForDelete);

                var result = await _storeScheduleRepository.Delete(existingSchedule);
                if (!result)
                    return (null, ConstantMessage.Common.DeleteFail);

                return (_mapper.Map<StoreScheduleModel>(existingSchedule), ConstantMessage.Common.DeleteSuccess);
            }
            catch (Exception ex)
            {
                return (null, $"Error while deleting store schedule: {ex.Message}");
            }
        }
    }
} 