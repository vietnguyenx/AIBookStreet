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
    public class RoleRepository : BaseRepository<Role>, IRoleRepository
    {
        private readonly BSDbContext _context;

        public RoleRepository(BSDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Role?> GetById(Guid id)
        {
            var query = GetQueryable(r => r.Id == id);
            var role = await query
                .SingleOrDefaultAsync();

            return role;
        }
    }
}
