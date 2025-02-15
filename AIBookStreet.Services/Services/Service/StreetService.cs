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
    public class StreetService(IUnitOfWork repository, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseService<Street>(mapper, repository, httpContextAccessor), IStreetService
    {
        private readonly IUnitOfWork _repository = repository;
        public async Task<(long, Street?)> AddAStreet(StreetModel model)
        {
            if (!string.IsNullOrEmpty(model.Address))
            {
                var existed = await _repository.StreetRepository.GetByAddress(model.Address);
                if (existed != null)
                {
                    return (1, null); //da ton tai
                }
            }
            var street = _mapper.Map<Street>(model);
            var setStreet = await SetBaseEntityToCreateFunc(street);
            var isSuccess = await _repository.StreetRepository.Add(setStreet);
            if (isSuccess)
            {
                return (2, setStreet);
            }
            return (3, null);
        }
        public async Task<(long, Street?)> UpdateAStreet(Guid? id, StreetModel model)
        {
            var existed = await _repository.StreetRepository.GetByID(id);
            if (existed == null)
            {
                return (1, null); //khong ton tai
            }
            if (existed.IsDeleted)
            {
                return (3, null);
            }
            existed.StreetName = model.StreetName;
            existed.Description = model.Description ?? existed.Description;
            existed.Address = model.Address ?? existed.Address;

            existed = await SetBaseEntityToUpdateFunc(existed);
            return await _repository.StreetRepository.Update(existed) ? (2, existed) //update thanh cong
                                                                          : (3, null);       //update fail
        }
        public async Task<(long, Street?)> DeleteAStreet(Guid id)
        {
            var existed = await _repository.StreetRepository.GetByID(id);
            if (existed == null)
            {
                return (1, null); //khong ton tai
            }
            existed = await SetBaseEntityToUpdateFunc(existed);

            return await _repository.StreetRepository.Delete(existed) ? (2, existed) //delete thanh cong
                                                                          : (3, null);       //delete fail
        }
        public async Task<Street?> GetAStreetById(Guid id)
        {
            return await _repository.StreetRepository.GetByID(id);
        }
        public async Task<(List<Street>?, long)> GetAllStreetsPagination(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc)
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
            var streets = isAdmin ? await _repository.StreetRepository.GetAllPaginationForAdmin(key, pageNumber, pageSize, sortField, desc)
                                                    : await _repository.StreetRepository.GetAllPagination(key, pageNumber, pageSize, sortField, desc);
            return streets.Item1.Count > 0 ? (streets.Item1, streets.Item2) : (null, 0);
        }
        public async Task<List<Street>?> GetAllActiveStreets()
        {
            var streets = await _repository.StreetRepository.GetAll();

            return streets.Count == 0 ? null : streets;
        }
    }
}
