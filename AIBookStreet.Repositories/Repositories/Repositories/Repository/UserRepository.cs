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
                .Include(u => u.BookStore)
                .Include(u => u.Publisher)
                .Include(u => u.UserRoles)
                .SingleOrDefaultAsync();

            return user;
        }

        public async Task<(List<User>, long)> Search(User user, int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var queryable = GetQueryable();
            queryable = base.ApplySort(queryable, sortField, sortOrder);

            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(user.UserName))
                {
                    queryable = queryable.Where(u => u.UserName.ToLower().Trim() == user.UserName.ToLower().Trim());
                }

                if (!string.IsNullOrEmpty(user.Email))
                {
                    queryable = queryable.Where(u => u.Email.ToLower().Trim() == user.Email.ToLower().Trim());
                }

                if (!string.IsNullOrEmpty(user.FullName))
                {
                    queryable = queryable.Where(u => u.FullName.ToLower().Trim().Contains(user.FullName.ToLower().Trim()));
                }

                if (user.DOB.HasValue)
                {
                    queryable = queryable.Where(u => u.DOB.Value.Date == user.DOB.Value.Date);
                }

                if (!string.IsNullOrEmpty(user.Address))
                {
                    queryable = queryable.Where(u => u.Address.ToLower().Trim().Contains(user.Address.ToLower().Trim()));
                }

                if (!string.IsNullOrEmpty(user.Phone))
                {
                    queryable = queryable.Where(u => u.Phone.ToLower().Trim().Contains(user.Phone.ToLower().Trim()));
                }

                if (!string.IsNullOrEmpty(user.Gender))
                {
                    queryable = queryable.Where(u => u.Gender.ToLower().Trim() == user.Gender.ToLower().Trim());
                }

            }

            var totalOrigin = queryable.Count();

            queryable = GetQueryablePagination(queryable, pageNumber, pageSize);

            var users = await queryable
                .Include(u => u.Images)
                .ToListAsync();

            return (users, totalOrigin);
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
                .Include(m => m.UserRoles).ThenInclude(ur => ur.Role)
                .Include(m => m.BookStore)
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
                .Include(m => m.UserRoles)
                .Include(m => m.BookStore)
                .Include(m => m.Publisher).SingleOrDefaultAsync();

            return result;
        }
    }
}
