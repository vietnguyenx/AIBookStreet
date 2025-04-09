using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IStoreScheduleService
    {
        Task<List<StoreScheduleModel>> GetAll();
        Task<StoreScheduleModel?> GetById(Guid id);
        Task<List<StoreScheduleModel>?> GetByStoreId(Guid storeId);
        Task<StoreScheduleModel?> GetByStoreIdAndDayOfWeek(Guid storeId, DayOfWeek dayOfWeek);
        Task<StoreScheduleModel?> GetByStoreIdAndSpecialDate(Guid storeId, DateTime specialDate);
        Task<(StoreScheduleModel?, string)> Add(StoreScheduleModel model);
        Task<(StoreScheduleModel?, string)> Update(StoreScheduleModel model);
        Task<(StoreScheduleModel?, string)> Delete(Guid id);
    }
} 