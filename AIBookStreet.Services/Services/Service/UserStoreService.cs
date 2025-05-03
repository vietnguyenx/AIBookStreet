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
    public class UserStoreService : BaseService<UserStore>, IUserStoreService
    {
        private readonly IUserStoreRepository _userStoreRepository;

        public UserStoreService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(mapper, unitOfWork, httpContextAccessor)
        {
            _userStoreRepository = unitOfWork.UserStoreRepository;
        }

        private async Task CheckAndUpdateExpiredContracts()
        {
            var now = DateTime.Now;
            var expiredContracts = await _userStoreRepository.GetAll();
            expiredContracts = expiredContracts.Where(x => x.EndDate != null && 
                                                         x.EndDate < now && 
                                                         x.Status != "Expired" && 
                                                         x.Status != "Terminated").ToList();

            foreach (var contract in expiredContracts)
            {
                contract.Status = "Expired";
                contract.LastUpdatedDate = now;
                await _userStoreRepository.Update(contract);
            }
        }

        public async Task<List<UserStoreModel>> GetAll()
        {
            await CheckAndUpdateExpiredContracts();
            var userStores = await _userStoreRepository.GetAll();

            if (!userStores.Any())
            {
                return null;
            }

            return _mapper.Map<List<UserStoreModel>>(userStores);
        }

        public async Task<List<UserStoreModel>?> GetByUserId(Guid userId)
        {
            await CheckAndUpdateExpiredContracts();
            var userStores = await _userStoreRepository.GetByUserId(userId);
            if (!userStores.Any()) return null;
            return _mapper.Map<List<UserStoreModel>>(userStores);
        }

        public async Task<List<UserStoreModel>?> GetByStoreId(Guid storeId)
        {
            await CheckAndUpdateExpiredContracts();
            var userStores = await _userStoreRepository.GetByStoreId(storeId);
            if (!userStores.Any()) return null;
            return _mapper.Map<List<UserStoreModel>>(userStores);
        }

        public async Task<bool> Add(UserStoreModel userStoreModel)
        {
            var userStore = await _userStoreRepository.GetByUserIdAndStoreId(userStoreModel.UserId, userStoreModel.StoreId);
            if (userStore != null) { return false; }
            var mappedUserStore = _mapper.Map<UserStore>(userStoreModel);
            var newUserStore = await SetBaseEntityToCreateFunc(mappedUserStore);
            return await _userStoreRepository.Add(newUserStore);
        }

        public async Task<bool> Delete(Guid userId, Guid storeId)
        {
            var userStore = await _userStoreRepository.GetByUserIdAndStoreId(userId, storeId);
            if (userStore == null)
            {
                return false;
            }

            var deleteUserStore = _mapper.Map<UserStore>(userStore);
            return await _userStoreRepository.Remove(deleteUserStore);
        }

        public async Task<bool> UpdateExpiredContracts()
        {
            await CheckAndUpdateExpiredContracts();
            return true;
        }
    }
}
