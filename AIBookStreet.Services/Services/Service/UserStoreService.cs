using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using AIBookStreet.Repositories.Repositories.Repositories.Repository;
using AIBookStreet.Repositories.Repositories.UnitOfWork.Interface;
using AIBookStreet.Services.Base;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
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
        private readonly IUserAccountEmailService _userAccountEmailService;
        private readonly IConfiguration _configuration;

        public UserStoreService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IFirebaseStorageService firebaseStorage,
            IUserAccountEmailService userAccountEmailService,
            IConfiguration configuration) : base(mapper, unitOfWork, httpContextAccessor)
        {
            _userStoreRepository = unitOfWork.UserStoreRepository;
            _firebaseStorage = firebaseStorage;
            _userAccountEmailService = userAccountEmailService;
            _configuration = configuration;
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
            {
                // Gửi email thông báo hợp đồng
                await SendContractNotificationEmail(userStoreModel);
                return (true, "Tạo hợp đồng thành công.");
            }
            else
                return (false, "Tạo hợp đồng thất bại.");
        }

        private async Task SendContractNotificationEmail(UserStoreModel userStoreModel)
        {
            try
            {
                // Lấy thông tin user và store với relationships
                var userStoreWithDetails = await _userStoreRepository.GetByUserIdAndStoreId(userStoreModel.UserId, userStoreModel.StoreId);
                
                if (userStoreWithDetails?.User == null || userStoreWithDetails?.Store == null)
                {
                    Console.WriteLine("Không thể lấy thông tin user hoặc store để gửi email");
                    return;
                }

                var user = userStoreWithDetails.User;
                var store = userStoreWithDetails.Store;

                if (string.IsNullOrEmpty(user.Email))
                {
                    Console.WriteLine($"User {user.UserName} không có email để gửi thông báo hợp đồng");
                    return;
                }

                Console.WriteLine($"Bắt đầu chuẩn bị email thông báo hợp đồng cho user {user.UserName} ({user.Email})");

                var emailModel = new ContractNotificationEmailModel
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName ?? user.UserName,
                    Phone = user.Phone ?? "Chưa cập nhật",
                    Address = user.Address ?? "Chưa cập nhật",
                    
                    StoreName = store.StoreName,
                    StoreAddress = store.Address ?? "Chưa cập nhật",
                    StoreType = store.Type ?? "Chưa cập nhật",
                    
                    StartDate = userStoreModel.StartDate,
                    EndDate = userStoreModel.EndDate,
                    Status = userStoreModel.Status ?? "Active",
                    ContractNumber = userStoreModel.ContractNumber ?? "Chưa có",
                    ContractFileUrl = userStoreModel.ContractFileUrl ?? "",
                    Notes = userStoreModel.Notes ?? "",
                    
                    CreatedDate = DateTime.Now,
                    LoginUrl = _configuration["AppSettings:LoginUrl"] ?? "https://smart-book-street-next-aso3.vercel.app/login",
                    BaseImgUrl = user.BaseImgUrl
                };

                Console.WriteLine($"Đã tạo email model, bắt đầu gửi email...");

                var emailSent = await _userAccountEmailService.SendContractNotificationEmailAsync(emailModel);
                if (emailSent)
                {
                    Console.WriteLine($"✅ Đã gửi email thông báo hợp đồng thuê store {store.StoreName} cho {user.Email} thành công");
                }
                else
                {
                    Console.WriteLine($"❌ Không thể gửi email thông báo hợp đồng thuê store {store.StoreName} cho {user.Email}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Lỗi khi gửi email thông báo hợp đồng: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
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

        public async Task<(byte[] fileData, string contentType, string fileName)?> DownloadContractFile(Guid userId, Guid storeId)
        {
            try
            {
                // Lấy thông tin hợp đồng
                var userStore = await _userStoreRepository.GetByUserIdAndStoreId(userId, storeId);
                if (userStore == null)
                {
                    throw new Exception("Không tìm thấy hợp đồng");
                }

                // Kiểm tra xem có file hợp đồng không
                if (string.IsNullOrEmpty(userStore.ContractFileUrl))
                {
                    throw new Exception("Hợp đồng này không có file đính kèm");
                }

                // Download file từ Firebase Storage
                var (fileData, contentType, fileName) = await _firebaseStorage.DownloadFileAsync(userStore.ContractFileUrl);
                
                return (fileData, contentType, fileName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải xuống file hợp đồng: {ex.Message}");
            }
        }
    }
}
