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
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly BSDbContext _context;
        public UserRepository(BSDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var queryable = GetQueryable();
            queryable = base.ApplySort(queryable, sortField, sortOrder);

            queryable = GetQueryablePagination(queryable, pageNumber, pageSize);

            return await queryable
                .Include(u => u.Images)
                .ToListAsync();
        }

        public async Task<User?> GetById(Guid id)
        {
            var query = GetQueryable(m => m.Id == id);
            var user = await query
                .Include(u => u.Images)
                .Include(u => u.UserStores)
                .Include(u => u.Publisher)
                .Include(u => u.UserRoles.Where(ur => !ur.IsDeleted)).ThenInclude(ur => ur.Role)
                .SingleOrDefaultAsync();

            return user;
        }

        public async Task<(List<User>, long)> SearchPagination(User user, int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var queryable = GetQueryable().Where(u => !u.IsDeleted);
            queryable = base.ApplySort(queryable, sortField, sortOrder);

            if (!string.IsNullOrEmpty(user.UserName))
            {
                queryable = queryable.Where(u => EF.Functions.Collate(u.UserName, "Latin1_General_CI_AI").Contains(user.UserName));
            }

            if (!string.IsNullOrEmpty(user.Email))
            {
                queryable = queryable.Where(u => EF.Functions.Collate(u.Email, "Latin1_General_CI_AI").Contains(user.Email));
            }

            if (!string.IsNullOrEmpty(user.FullName))
            {
                queryable = queryable.Where(u => EF.Functions.Collate(u.FullName, "Latin1_General_CI_AI").Contains(user.FullName));
            }

            if (user.DOB.HasValue)
            {
                queryable = queryable.Where(u => u.DOB.Value.Date == user.DOB.Value.Date);
            }

            if (!string.IsNullOrEmpty(user.Address))
            {
                queryable = queryable.Where(u => EF.Functions.Collate(u.Address, "Latin1_General_CI_AI").Contains(user.Address));
            }

            if (!string.IsNullOrEmpty(user.Phone))
            {
                queryable = queryable.Where(u => EF.Functions.Collate(u.Phone, "Latin1_General_CI_AI").Contains(user.Phone));
            }

            if (!string.IsNullOrEmpty(user.Gender))
            {
                queryable = queryable.Where(u => EF.Functions.Collate(u.Gender, "Latin1_General_CI_AI") == user.Gender);
            }

            var totalOrigin = await queryable.CountAsync();

            queryable = GetQueryablePagination(queryable, pageNumber, pageSize);

            var users = await queryable
                .Include(u => u.Images)
                .Include(u => u.UserStores)
                .Include(u => u.Publisher)
                .Include(u => u.UserRoles.Where(ur => !ur.IsDeleted)).ThenInclude(ur => ur.Role)
                .ToListAsync();

            return (users, totalOrigin);
        }

        public async Task<List<User>> SearchWithoutPagination(User user)
        {
            var queryable = GetQueryable().Where(u => !u.IsDeleted);

            if (!string.IsNullOrEmpty(user.UserName))
            {
                queryable = queryable.Where(u => EF.Functions.Collate(u.UserName, "Latin1_General_CI_AI").Contains(user.UserName));
            }

            if (!string.IsNullOrEmpty(user.Email))
            {
                queryable = queryable.Where(u => EF.Functions.Collate(u.Email, "Latin1_General_CI_AI").Contains(user.Email));
            }

            if (!string.IsNullOrEmpty(user.FullName))
            {
                queryable = queryable.Where(u => EF.Functions.Collate(u.FullName, "Latin1_General_CI_AI").Contains(user.FullName));
            }

            if (user.DOB.HasValue)
            {
                queryable = queryable.Where(u => u.DOB.Value.Date == user.DOB.Value.Date);
            }

            if (!string.IsNullOrEmpty(user.Address))
            {
                queryable = queryable.Where(u => EF.Functions.Collate(u.Address, "Latin1_General_CI_AI").Contains(user.Address));
            }

            if (!string.IsNullOrEmpty(user.Phone))
            {
                queryable = queryable.Where(u => EF.Functions.Collate(u.Phone, "Latin1_General_CI_AI").Contains(user.Phone));
            }

            if (!string.IsNullOrEmpty(user.Gender))
            {
                queryable = queryable.Where(u => EF.Functions.Collate(u.Gender, "Latin1_General_CI_AI") == user.Gender);
            }

            return await queryable
                .Include(u => u.Images)
                .Include(u => u.UserStores)
                .Include(u => u.Publisher)
                .Include(u => u.UserRoles.Where(ur => !ur.IsDeleted)).ThenInclude(ur => ur.Role)
                .ToListAsync();
        }


        public async Task<User> FindUsernameOrEmail(User user)
        {
            var queryable = base.GetQueryable();
            queryable = queryable.Where(entity => !entity.IsDeleted);

            if (!string.IsNullOrEmpty(user.UserName) || !string.IsNullOrEmpty(user.Email))
            {
                queryable = queryable.Where(entity => user.UserName.ToLower() == entity.UserName.ToLower()
                                            || user.Email.ToLower() == entity.Email.ToLower()
                );
            }

            var result = await queryable
                .Include(m => m.UserRoles.Where(ur => !ur.IsDeleted)).ThenInclude(ur => ur.Role)
                .Include(m => m.UserStores)
                .Include(m => m.Publisher).SingleOrDefaultAsync();

            return result;
        }

        public async Task<User> GetUserByEmail(User user)
        {
            var queryable = base.GetQueryable();
            queryable = queryable.Where(entity => !entity.IsDeleted);

            if (!string.IsNullOrEmpty(user.Email))
            {
                queryable = queryable.Where(entity => user.Email.ToLower() == entity.Email.ToLower()
                );
            }

            var result = await queryable
                .Include(m => m.UserRoles.Where(ur => !ur.IsDeleted)).ThenInclude(ur => ur.Role)
                .Include(m => m.UserStores)
                .Include(m => m.Publisher).SingleOrDefaultAsync();

            return result;
        }
    }
}
