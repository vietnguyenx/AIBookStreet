using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Data;
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
using Microsoft.EntityFrameworkCore;

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
            
            // Đảm bảo rằng IsApproved được đặt khi tạo mới
            if (userRoleModel.IsApproved)
            {
                mappedUserRole.IsApproved = true;
            }
            else 
            {
                mappedUserRole.IsApproved = false;
            }
            
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
            return await _userRoleRepository.Remove(deleteUserRole);
        }

        public async Task<bool> ApproveRole(Guid userId, Guid roleId, bool approve)
        {
            var userRole = await _userRoleRepository.GetByUserIdAndRoleId(userId, roleId);
            if (userRole == null)
            {
                return false;
            }

            if (approve)
            {
                // Nếu phê duyệt, cập nhật IsApproved = true
                userRole.IsApproved = true;
                userRole.LastUpdatedDate = DateTime.Now;
                return await _userRoleRepository.Update(userRole);
            }
            else
            {
                // Nếu từ chối, xóa userRole khỏi database
                return await _userRoleRepository.Remove(userRole);
            }
        }

        public async Task<List<UserRoleModel>?> GetPendingRoleRequests()
        {
            var userRoles = await _userRoleRepository.GetAll();
            var pendingRoles = userRoles.Where(ur => !ur.IsApproved).ToList();
            
            if (!pendingRoles.Any())
            {
                return null;
            }

            return _mapper.Map<List<UserRoleModel>>(pendingRoles);
        }
    }
}
