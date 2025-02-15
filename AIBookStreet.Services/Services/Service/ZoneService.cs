using AIBookStreet.Repositories.Data.Entities;
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
    public class ZoneService(IUnitOfWork repository, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseService<Zone>(mapper, repository, httpContextAccessor), IZoneService
    {
        private readonly IUnitOfWork _repository = repository;
        public async Task<Zone?> AddAZone(ZoneModel model)
        {
            var zone = _mapper.Map<Zone>(model);
            var setZone = await SetBaseEntityToCreateFunc(zone);
            var isSuccess = await _repository.ZoneRepository.Add(setZone);
            if (isSuccess)
            {
                return setZone;
            }
            return null;
        }
        public async Task<(long, Zone?)> UpdateAZone(Guid? id, ZoneModel model)
        {
            var existed = await _repository.ZoneRepository.GetByID(id);
            if (existed == null)
            {
                return (1, null); //khong ton tai
            }
            if (existed.IsDeleted)
            {
                return (3, null);
            }
            existed.ZoneName = model.ZoneName;
            existed.Description = model.Description ?? existed.Description;
            existed.StreetId = model.StreetId ?? existed.StreetId;
            existed = await SetBaseEntityToUpdateFunc(existed);
            return await _repository.ZoneRepository.Update(existed) ? (2, existed) //update thanh cong
                                                                          : (3, null);       //update fail
        }
        public async Task<(long, Zone?)> DeleteAZone(Guid id)
        {
            var existed = await _repository.ZoneRepository.GetByID(id);
            if (existed == null)
            {
                return (1, null); //khong ton tai
            }
            existed = await SetBaseEntityToUpdateFunc(existed);

            return await _repository.ZoneRepository.Delete(existed) ? (2, existed) //delete thanh cong
                                                                          : (3, null);       //delete fail
        }
        public async Task<Zone?> GetAZoneById(Guid id)
        {
            return await _repository.ZoneRepository.GetByID(id);
        }
        public async Task<List<Zone>?> GetAllActiveZones()
        {
            var zones = await _repository.ZoneRepository.GetAll();

            return zones.Count == 0 ? null : zones;
        }
        public async Task<(List<Zone>?, long)> GetAllZonesPagination(string? key, Guid? streetID, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var user = await GetUserInfo();
            var isAdmin = false;
            if (user != null)
            {
                foreach (var userRole in user.UserRoles)
                {
                    if (userRole.Role.RoleName == "Quản trị viên")
                    {
                        isAdmin = true;
                    }
                }
            }
            var zones = isAdmin ? await _repository.ZoneRepository.GetAllPaginationForAdmin(key, streetID, pageNumber, pageSize, sortField, desc)
                                                  : await _repository.ZoneRepository.GetAllPagination(key, streetID, pageNumber, pageSize, sortField, desc);
            return zones.Item1.Count > 0 ? (zones.Item1, zones.Item2) : (null, 0);
        }
    }
}
