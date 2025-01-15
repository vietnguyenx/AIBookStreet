using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<List<User>> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<User?> GetById(Guid id);
        Task<(List<User>, long)> Search(User user, int pageNumber, int pageSize, string sortField, int sortOrder);

        Task<User> FindUsernameOrEmail(User user);
        Task<User> GetUserByEmail(User user);
    }
}
