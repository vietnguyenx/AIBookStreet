using AIBookStreet.Repositories.Data;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Repository
{
    public class StoreScheduleRepository : BaseRepository<StoreSchedule>, IStoreScheduleRepository
    {
        private readonly BSDbContext _context;

        public StoreScheduleRepository(BSDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<StoreSchedule>> GetByStoreId(Guid storeId)
        {
            return await GetQueryable(ss => ss.StoreId == storeId)
                .Include(ss => ss.Store)
                .ToListAsync();
        }

        public async Task<StoreSchedule?> GetByStoreIdAndDayOfWeek(Guid storeId, DayOfWeek dayOfWeek)
        {
            return await GetQueryable(ss => ss.StoreId == storeId && ss.DayOfWeek == dayOfWeek)
                .Include(ss => ss.Store)
                .FirstOrDefaultAsync();
        }

        public async Task<StoreSchedule?> GetByStoreIdAndSpecialDate(Guid storeId, DateTime specialDate)
        {
            return await GetQueryable(ss => ss.StoreId == storeId && ss.SpecialDate == specialDate)
                .Include(ss => ss.Store)
                .FirstOrDefaultAsync();
        }
    }
} 