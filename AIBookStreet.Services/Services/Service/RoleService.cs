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
    public class RoleService : BaseService<Role>, IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(mapper, unitOfWork, httpContextAccessor)
        {
            _roleRepository = unitOfWork.RoleRepository;
        }

        public async Task<List<RoleModel>> GetAll()
        {
            var roles = await _roleRepository.GetAll();

            if (!roles.Any())
            {
                return null;
            }

            return _mapper.Map<List<RoleModel>>(roles);
        }

        public async Task<RoleModel?> GetById(Guid id)
        {
            var role = await _roleRepository.GetById(id);

            if (role == null)
            {
                return null;
            }

            return _mapper.Map<RoleModel>(role);
        }

        public async Task<bool> Add(RoleModel roleModel)
        {
            var mappedRole = _mapper.Map<Role>(roleModel);
            var newRole = await SetBaseEntityToCreateFunc(mappedRole);
            return await _roleRepository.Add(newRole);
        }

        public async Task<bool> Delete(Guid roleId)
        {
            var existingRole = await _roleRepository.GetById(roleId);
            if (existingRole == null)
            {
                return false;
            }

            var mappedRole = _mapper.Map<Role>(existingRole);
            return await _roleRepository.Delete(mappedRole);
        }
    }
}
