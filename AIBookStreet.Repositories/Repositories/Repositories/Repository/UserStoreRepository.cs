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
    public class UserStoreRepository : BaseRepository<UserStore>, IUserStoreRepository
    {
        private readonly BSDbContext _context;

        public UserStoreRepository(BSDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<UserStore?>> GetByUserId(Guid userId)
        {
            var query = GetQueryable(ur => ur.UserId == userId);
            return await query
                .Include(ur => ur.User)
                .Include(i => i.Store)
                .ToListAsync();
        }

        public async Task<List<UserStore?>> GetByStoreId(Guid storeId)
        {
            var query = GetQueryable(ur => ur.StoreId == storeId);
            return await query
                .Include(ur => ur.User)
                .Include(ur => ur.Store)
                .ToListAsync();
        }

        public async Task<UserStore?> GetByUserIdAndStoreId(Guid userId, Guid storeId)
        {
            UserStore userStore = await _context.UserStores.Where(x => x.UserId == userId && x.StoreId == storeId)
                .Include(i => i.User)
                .Include(i => i.Store)
                .SingleOrDefaultAsync();
            return userStore;
        }

        public async Task<bool> IsStoreActiveForOtherUser(Guid storeId, Guid userId)
        {
            return await _context.UserStores.AnyAsync(x => x.StoreId == storeId && x.UserId != userId && x.Status == "Active");
        }

        public async Task<bool> UpdateExpiredContracts()
        {
            try
            {
                var now = DateTime.Now;
                var expiredContracts = await _context.UserStores
                    .Where(x => x.EndDate != null && 
                               x.EndDate < now && 
                               x.Status != "Expired")
                    .ToListAsync();

                foreach (var contract in expiredContracts)
                {
                    contract.Status = "Expired";
                    contract.LastUpdatedDate = now;
                }

                if (expiredContracts.Any())
                {
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
