using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface IStoreScheduleRepository : IBaseRepository<StoreSchedule>
    {
        Task<List<StoreSchedule>> GetByStoreId(Guid storeId);
        Task<StoreSchedule?> GetByStoreIdAndDayOfWeek(Guid storeId, DayOfWeek dayOfWeek);
        Task<StoreSchedule?> GetByStoreIdAndSpecialDate(Guid storeId, DateTime specialDate);
    }
} 