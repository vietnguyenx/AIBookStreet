using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using AIBookStreet.Repositories.Repositories.Repositories.Repository;
using AIBookStreet.Repositories.Repositories.UnitOfWork.Interface;
using AIBookStreet.Services.Base;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Service
{
    public class UserRoleService : BaseService<UserRole>, IUserRoleService
    {
        private readonly IUserRoleRepository _userRoleRepository;

        public UserRoleService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(mapper, unitOfWork, httpContextAccessor)
        {
            _userRoleRepository = unitOfWork.UserRoleRepository;
        }

        public async Task<List<UserRoleModel>> GetAll()
        {
            var userRoles = await _userRoleRepository.GetAll();

            if (!userRoles.Any())
            {
                return null;
            }

            return _mapper.Map<List<UserRoleModel>>(userRoles);
        }

        public async Task<List<UserRoleModel>?> GetByUserId(Guid userId)
        {
            var userRoles = await _userRoleRepository.GetByUserId(userId);
            if (!userRoles.Any()) return null;
            return _mapper.Map<List<UserRoleModel>>(userRoles);
        }

        public async Task<List<UserRoleModel>?> GetByRoleId(Guid roleId)
        {
            var userRoles = await _userRoleRepository.GetByRoleId(roleId);
            if (!userRoles.Any()) return null;
            return _mapper.Map<List<UserRoleModel>>(userRoles);
        }

        public async Task<bool> Add(UserRoleModel userRoleModel)
        {
            var userRole = await _userRoleRepository.GetByUserIdAndRoleId(userRoleModel.UserId, userRoleModel.RoleId);
            if (userRole != null) { return false; }
            var mappedUserRole = _mapper.Map<UserRole>(userRoleModel);
            var newUserRole = await SetBaseEntityToCreateFunc(mappedUserRole);
            return await _userRoleRepository.Add(newUserRole);
        }

        public async Task<bool> Delete(Guid userId, Guid roleId)
        {
            var userRole = await _userRoleRepository.GetByUserIdAndRoleId(userId, roleId);
            if (userRole == null)
            {
                return false;
            }

            var deleteUserRole = _mapper.Map<UserRole>(userRole);
            return await _userRoleRepository.Delete(deleteUserRole);
        }
    }
}
