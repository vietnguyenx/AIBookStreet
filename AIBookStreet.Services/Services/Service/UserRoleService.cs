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
using Microsoft.Extensions.Configuration;

namespace AIBookStreet.Services.Services.Service
{
    public class UserRoleService : BaseService<UserRole>, IUserRoleService
    {
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IUserAccountEmailService _userAccountEmailService;
        private readonly IConfiguration _configuration;

        public UserRoleService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor, IUserAccountEmailService userAccountEmailService, IConfiguration configuration) : base(mapper, unitOfWork, httpContextAccessor)
        {
            _userRoleRepository = unitOfWork.UserRoleRepository;
            _userAccountEmailService = userAccountEmailService;
            _configuration = configuration;
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

            // Lưu thông tin để gửi email
            var user = userRole.User;
            var role = userRole.Role;

            if (approve)
            {
                // Nếu phê duyệt, cập nhật IsApproved = true
                userRole.IsApproved = true;
                userRole.LastUpdatedDate = DateTime.Now;
                var updateResult = await _userRoleRepository.Update(userRole);
                
                if (updateResult)
                {
                    // Gửi email thông báo phê duyệt
                    await SendRoleApprovalEmail(user, role, true);
                }
                
                return updateResult;
            }
            else
            {
                // Nếu từ chối, xóa userRole khỏi database
                var deleteResult = await _userRoleRepository.Remove(userRole);
                
                if (deleteResult)
                {
                    // Gửi email thông báo từ chối
                    await SendRoleApprovalEmail(user, role, false);
                }
                
                return deleteResult;
            }
        }

        private async Task SendRoleApprovalEmail(User user, Role role, bool isApproved)
        {
            try
            {
                if (string.IsNullOrEmpty(user.Email))
                {
                    Console.WriteLine($"Không thể gửi email: User {user.UserName} không có email");
                    return;
                }

                var emailModel = new RoleApprovalEmailModel
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName ?? user.UserName,
                    RoleName = role.RoleName,
                    IsApproved = isApproved,
                    DecisionDate = DateTime.Now,
                    LoginUrl = _configuration["AppSettings:LoginUrl"] ?? "https://smart-book-street-next-aso3.vercel.app/login",
                    BaseImgUrl = user.BaseImgUrl
                };

                var emailSent = await _userAccountEmailService.SendRoleApprovalEmailAsync(emailModel);
                if (emailSent)
                {
                    Console.WriteLine($"Đã gửi email thông báo {(isApproved ? "phê duyệt" : "từ chối")} role {role.RoleName} cho {user.Email}");
                }
                else
                {
                    Console.WriteLine($"Không thể gửi email thông báo {(isApproved ? "phê duyệt" : "từ chối")} role cho {user.Email}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gửi email thông báo role: {ex.Message}");
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
