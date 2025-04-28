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
    public class SouvenirService(IUnitOfWork repository, IMapper mapper, IHttpContextAccessor httpContextAccessor, IFirebaseStorageService firebaseStorageService, IImageService imageService) : BaseService<Souvenir>(mapper, repository, httpContextAccessor), ISouvenirService
    {
        private readonly IUnitOfWork _repository = repository;
        private readonly IFirebaseStorageService _firebaseStorageService = firebaseStorageService;
        private readonly IImageService _imageService = imageService;
        public async Task<Souvenir?> AddASouvenir(SouvenirModel model)
        {
            try
            {
                var baseFileUrl = "";
                if (model.BaseImgFile != null)
                {
                    baseFileUrl = await _firebaseStorageService.UploadFileAsync(model.BaseImgFile);
                }

                var souvenir = new Souvenir
                {
                    BaseImgUrl = !string.IsNullOrEmpty(baseFileUrl) ? baseFileUrl : null,
                    SouvenirName = model.SouvenirName,
                    Description = model.Description ?? null,
                    Price = model.Price,
                };

                var setSouvenir = await SetBaseEntityToCreateFunc(souvenir);
                var isSuccess = await _repository.SouvenirRepository.Add(setSouvenir);

                if (!isSuccess)
                {
                    if (model.BaseImgFile != null)
                    {
                        await _firebaseStorageService.DeleteFileAsync(baseFileUrl);
                    }
                    return null;
                }

                if (model.OtherImgFiles != null)
                {
                    var images = new List<FileModel>();
                    foreach (var imageFile in model.OtherImgFiles)
                    {
                        var image = new FileModel
                        {
                            File = imageFile,
                            Type = "souvenir_additional",
                            AltText = "Ảnh quà lưu niệm " + setSouvenir.SouvenirName,
                            EntityId = setSouvenir.Id
                        };
                        images.Add(image);
                    }
                    var result = _imageService.AddImages(images);
                    if (result == null)
                    {
                        return null;
                    }
                }
                return setSouvenir;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<(long, Souvenir?)> UpdateASouvenir(Guid id, SouvenirModel model)
        {
            var existed = await _repository.SouvenirRepository.GetByID(id);
            if (existed == null)
            {
                return (1, null); //khong ton tai
            }
            if (existed.IsDeleted)
            {
                return (3, null);
            }
            try
            {
                var newFileUrl = model.BaseImgFile != null ? await _firebaseStorageService.UploadFileAsync(model.BaseImgFile) : "";

                var oldFileUrl = existed.BaseImgUrl;
                // Update entity
                existed.SouvenirName = model.SouvenirName;
                existed.Description = model.Description ?? existed.Description;
                existed.BaseImgUrl = newFileUrl == "" ? existed.BaseImgUrl : newFileUrl;
                existed.Price = model.Price;
                existed = await SetBaseEntityToUpdateFunc(existed);

                var updateSuccess = await _repository.SouvenirRepository.Update(existed);
                if (updateSuccess)
                {
                    if (model.BaseImgFile != null && !string.IsNullOrEmpty(oldFileUrl))
                    {
                        await _firebaseStorageService.DeleteFileAsync(oldFileUrl);
                    }
                    return (2, existed); //update thành công
                }

                // Cleanup new file if update fails
                if (model.BaseImgFile != null)
                {
                    await _firebaseStorageService.DeleteFileAsync(newFileUrl);
                }
                return (3, null); //update fail
            }
            catch
            {
                throw;
            }
        }
        public async Task<(long, Souvenir?)> DeleteASouvenir(Guid id)
        {
            var existed = await _repository.SouvenirRepository.GetByID(id);
            if (existed == null)
            {
                return (1, null); //khong ton tai
            }
            if (existed.IsDeleted)
            {
                return (3, null);
            }
            existed = await SetBaseEntityToUpdateFunc(existed);

            var isSuccess = await _repository.SouvenirRepository.Delete(existed);
            if (isSuccess)
            {
                try
                {
                    if (!string.IsNullOrEmpty(existed.BaseImgUrl))
                    {
                        await _firebaseStorageService.DeleteFileAsync(existed.BaseImgUrl);
                    }
                    var otherImages = existed.Images;
                    if (otherImages != null)
                    {
                        foreach (var image in otherImages)
                        {
                            await _firebaseStorageService.DeleteFileAsync(image.Url);
                        }
                    }
                }
                catch
                {
                    throw;
                }
                return (2, existed);//delete thanh cong
            }
            return (3, null);       //delete fail
        }
        public async Task<Souvenir?> GetASouvenirById(Guid id)
        {
            return await _repository.SouvenirRepository.GetByID(id);
        }

        public async Task<(List<Souvenir>?, long)> GetAllSouvenirsPagination(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var user = await GetUserInfo();
            var isAdmin = false;
            if (user != null)
            {
                foreach (var userRole in user.UserRoles)
                {
                    if (userRole.Role.RoleName == "Admin")
                    {
                        isAdmin = true;
                    }
                }
            }
            var souvenirs = isAdmin ? await _repository.SouvenirRepository.GetAllPaginationForAdmin(key, pageNumber, pageSize, sortField, desc)
                                                       : await _repository.SouvenirRepository.GetAllPagination(key, pageNumber, pageSize, sortField, desc);
            return souvenirs.Item1.Count > 0 ? (souvenirs.Item1, souvenirs.Item2) : (null, 0);
        }

    }
}
