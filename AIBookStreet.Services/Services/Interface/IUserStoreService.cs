using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IUserStoreService
    {
        Task<List<UserStoreModel>> GetAll();
        Task<List<UserStoreModel>?> GetByUserId(Guid userId);
        Task<List<UserStoreModel>?> GetByStoreId(Guid storeId);
        Task<(bool isSuccess, string message)> Add(UserStoreModel userStoreModel);
        Task<bool> Delete(Guid idUser, Guid idStore);
        Task<bool> UpdateExpiredContracts();
        Task<(int totalSent, string message)> SendExpirationWarningEmails();
        Task<(byte[] fileData, string contentType, string fileName)?> DownloadContractFile(Guid userId, Guid storeId);
    }
}
