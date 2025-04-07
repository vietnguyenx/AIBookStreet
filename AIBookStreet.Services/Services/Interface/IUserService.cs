using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIBookStreet.Repositories.Data.Entities;
using System.Security.Claims;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IUserService
    {
        Task<List<UserModel>?> GetAll();
        Task<List<UserModel>?> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<UserModel?> GetById(Guid id);
        Task<(List<UserModel>?, long)> SearchPagination(UserModel userModel, int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<List<UserModel>?> SearchWithoutPagination(UserModel userModel);
        Task<(UserModel?, string)> Add(UserModel userModel);
        Task<(UserModel?, string)> Update(UserModel userModel);
        Task<(UserModel?, string)> Delete(Guid userId);
        Task<long> GetTotalCount();

        Task<UserModel> Login(AuthModel authModel);
        Task<UserModel> Register(UserModel userModel);
        JwtSecurityToken CreateToken(UserModel userModel);
        Task<UserModel?> GetUserByEmailOrUsername(UserModel userModel);
        Task<UserModel?> GetUserByEmail(UserModel userModel);
        Task<User?> GetUserInfo();
        Task<UserModel?> ProcessGoogleLoginAsync(ClaimsPrincipal claimsPrincipal);
    }
}
