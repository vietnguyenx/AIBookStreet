using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIBookStreet.Repositories.Repositories.Base;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace AIBookStreet.Repositories.Repositories.Repositories.Repository
{
    public class UserRoleRepository : BaseRepository<UserRole>, IUserRoleRepository
    {
        private readonly BSDbContext _context;

        public UserRoleRepository(BSDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<UserRole?>> GetByUserId(Guid userId)
        {
            var query = GetQueryable(ur => ur.UserId == userId);
            return await query
                .Include(ur => ur.User)
                .Include(i => i.Role)
                .ToListAsync();
        }

        public async Task<List<UserRole?>> GetByRoleId(Guid roleId)
        {
            var query = GetQueryable(ur => ur.RoleId == roleId);
            return await query
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .ToListAsync();
        }

        public async Task<UserRole?> GetByUserIdAndRoleId(Guid userId, Guid roleId)
        {
            UserRole userRole = await _context.UserRoles.Where(x => x.UserId == userId && x.RoleId == roleId)
                .Include(i => i.User)
                .Include(i => i.Role)
                .SingleOrDefaultAsync();
            return userRole;
        }
    }
}
