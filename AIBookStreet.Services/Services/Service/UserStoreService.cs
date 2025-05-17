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
        private readonly IFirebaseStorageService _firebaseStorage;

        public UserStoreService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IFirebaseStorageService firebaseStorage) : base(mapper, unitOfWork, httpContextAccessor)
        {
            _userStoreRepository = unitOfWork.UserStoreRepository;
            _firebaseStorage = firebaseStorage;
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

        public async Task<(bool isSuccess, string message)> Add(UserStoreModel userStoreModel)
        {
            // Kiểm tra store đã có hợp đồng active với user khác chưa
            bool isActiveForOtherUser = await _userStoreRepository.IsStoreActiveForOtherUser(userStoreModel.StoreId, userStoreModel.UserId);
            if (isActiveForOtherUser)
            {
                return (false, "Cửa hàng đã chọn đã có hợp đồng đang hoạt động với người dùng khác.");
            }
            var userStore = await _userStoreRepository.GetByUserIdAndStoreId(userStoreModel.UserId, userStoreModel.StoreId);
            if (userStore != null) { return (false, "Đã tồn tại hợp đồng thuê giữa người dùng và cửa hàng đã chọn."); }

            // Upload file hợp đồng nếu có
            if (userStoreModel.ContractFile != null)
            {
                try
                {
                    // Kiểm tra kích thước file (max 10MB)
                    if (userStoreModel.ContractFile.Length > 10 * 1024 * 1024)
                        return (false, "Kích thước file hợp đồng vượt quá 10MB");

                    // Kiểm tra định dạng file
                    var allowedTypes = new[] { "application/pdf", "image/jpeg", "image/png" };
                    if (!allowedTypes.Contains(userStoreModel.ContractFile.ContentType.ToLower()))
                        return (false, "Định dạng file không được hỗ trợ. Chỉ chấp nhận PDF, JPEG, PNG");

                    // Upload file lên Firebase Storage
                    var fileUrl = await _firebaseStorage.UploadFileAsync(userStoreModel.ContractFile);
                    userStoreModel.ContractFileUrl = fileUrl;
                }
                catch (Exception ex)
                {
                    return (false, $"Lỗi khi upload file hợp đồng: {ex.Message}");
                }
            }

            var mappedUserStore = _mapper.Map<UserStore>(userStoreModel);
            var newUserStore = await SetBaseEntityToCreateFunc(mappedUserStore);
            bool result = await _userStoreRepository.Add(newUserStore);
            if (result)
                return (true, "Tạo hợp đồng thành công.");
            else
                return (false, "Tạo hợp đồng thất bại.");
        }

        public async Task<bool> Delete(Guid userId, Guid storeId)
        {
            var userStore = await _userStoreRepository.GetByUserIdAndStoreId(userId, storeId);
            if (userStore == null)
            {
                return false;
            }

            // Xóa file hợp đồng nếu có
            if (!string.IsNullOrEmpty(userStore.ContractFileUrl))
            {
                try
                {
                    await _firebaseStorage.DeleteFileAsync(userStore.ContractFileUrl);
                }
                catch
                {
                    // Bỏ qua lỗi khi xóa file
                }
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
