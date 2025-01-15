using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IUserService
    {
        public Task<List<UserModel>?> GetAll();

        public Task<List<UserModel>?> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder);
        public Task<UserModel?> GetById(Guid id);
        public Task<(List<UserModel>?, long)> Search(UserModel userModel, int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<bool> Add(UserModel userModel);
        Task<bool> Update(UserModel userModel);
        Task<bool> Delete(Guid id);
        public Task<long> GetTotalCount();

        Task<UserModel> Login(AuthModel authModel);
        Task<UserModel> Register(UserModel userModel);
        public JwtSecurityToken CreateToken(UserModel userModel);
        public Task<UserModel?> GetUserByEmailOrUsername(UserModel userModel);
        public Task<UserModel?> GetUserByEmail(UserModel userModel);

    }
}
