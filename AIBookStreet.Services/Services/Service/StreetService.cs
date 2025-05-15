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
    public class StreetService(IUnitOfWork repository, IMapper mapper, IHttpContextAccessor httpContextAccessor, IFirebaseStorageService firebaseStorageService, IImageService imageService) : BaseService<Street>(mapper, repository, httpContextAccessor), IStreetService
    {
        private readonly IUnitOfWork _repository = repository;
        private readonly IFirebaseStorageService _firebaseStorageService = firebaseStorageService;
        private readonly IImageService _imageService = imageService;
        public async Task<(long, Street?)> AddAStreet(StreetModel model)
        {
            try
            {
                var baseFileUrl = "";
                if (model.BaseImgFile != null)
                {
                    baseFileUrl = await _firebaseStorageService.UploadFileAsync(model.BaseImgFile);
                }

                var street = new Street
                {
                    BaseImgUrl = !string.IsNullOrEmpty(baseFileUrl) ? baseFileUrl : null,
                    StreetName = model.StreetName,
                    Description = model.Description ?? null,
                    Address = model.Address ?? null,
                    Latitude = model.Latitude ?? null,
                    Longitude = model.Longitude ?? null,
                };

                var setStreet = await SetBaseEntityToCreateFunc(street);
                var isSuccess = await _repository.StreetRepository.Add(setStreet);

                if (!isSuccess)
                {
                    if (model.BaseImgFile != null)
                    {
                        await _firebaseStorageService.DeleteFileAsync(baseFileUrl);
                    }
                    return (3, null);
                }

                if (model.OtherImgFiles != null)
                {
                    var images = new List<FileModel>();
                    foreach (var imageFile in model.OtherImgFiles)
                    {
                        var image = new FileModel
                        {
                            File = imageFile,
                            Type = "Đường sách",
                            AltText = "Ảnh đường sách " + setStreet.StreetName,
                            EntityId = setStreet.Id
                        };
                        images.Add(image);
                    }
                    var result = _imageService.AddImages(images);
                    if (result == null)
                    {
                        return (3, null);
                    }
                }
                return (2, setStreet);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<(long, Street?)> UpdateAStreet(Guid id, StreetModel model)
        {            
            try
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
                var newFileUrl = model.BaseImgFile != null ? await _firebaseStorageService.UploadFileAsync(model.BaseImgFile) : "";

                var oldFileUrl = existed.BaseImgUrl;
                // Update entity
                existed.StreetName = model.StreetName;
                existed.Description = model.Description ?? existed.Description;
                existed.Address = model.Address ?? existed.Address;
                existed.BaseImgUrl = newFileUrl == "" ? existed.BaseImgUrl : newFileUrl;
                existed.Latitude = model.Latitude ?? existed.Latitude;
                existed.Longitude = model.Longitude ?? existed.Longitude;
                existed = await SetBaseEntityToUpdateFunc(existed);

                var updateSuccess = await _repository.StreetRepository.Update(existed);
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
        public async Task<(long, Street?)> DeleteAStreet(Guid id)
        {
            try
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
                existed = await SetBaseEntityToUpdateFunc(existed);

                var isSuccess = await _repository.StreetRepository.Delete(existed);
                if (isSuccess)
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
                    return (2, existed);//delete thanh cong
                }
                return (3, null);       //delete fail
            } catch
            {
                throw;
            }
        }
        public async Task<Street?> GetAStreetById(Guid id)
        {
            try
            {
                return await _repository.StreetRepository.GetByID(id);
            } catch
            {
                throw;
            }
        }
        public async Task<(List<Street>?, long)> GetAllStreetsPagination(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            try
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
                var streets = isAdmin ? await _repository.StreetRepository.GetAllPaginationForAdmin(key, pageNumber, pageSize, sortField, desc)
                                                        : await _repository.StreetRepository.GetAllPagination(key, pageNumber, pageSize, sortField, desc);
                return streets.Item1.Count > 0 ? (streets.Item1, streets.Item2) : (null, 0);
            } catch
            {
                throw;
            }
        }
        public async Task<List<Street>?> GetAllActiveStreets()
        {
            try
            {
                var streets = await _repository.StreetRepository.GetAll();

                return streets.Count == 0 ? null : streets;
            } catch
            {
                throw;
            }
        }
    }
}
