using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.UnitOfWork.Interface;
using AIBookStreet.Services.Base;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AIBookStreet.Services.Services.Service
{
    public class ImageService : BaseService<Image>, IImageService
    {
        private readonly IUnitOfWork _repository;
        private readonly IFirebaseStorageService _firebaseStorage;

        public ImageService(
            IUnitOfWork repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IFirebaseStorageService firebaseStorage) : base(mapper, repository, httpContextAccessor)
        {
            _repository = repository;
            _firebaseStorage = firebaseStorage;
        }

        public async Task<List<Image>?> AddImages(List<FileModel> models)
        {
            var images = new List<Image>();
            foreach (var model in models)
            {
                try
                {
                    // Upload to Firebase
                    var fileUrl = await _firebaseStorage.UploadFileAsync(model.File);

                    // Create image entity
                    var image = new Image
                    {
                        Url = fileUrl,
                        Type = model.Type,
                        AltText = model.AltText ?? model.File.FileName,
                        EntityId = model.EntityId
                    };

                    var setImage = await SetBaseEntityToCreateFunc(image);
                    var isSuccess = await _repository.ImageRepository.Add(setImage);
                    
                    if (!isSuccess)
                    {
                        // Cleanup: delete uploaded file if database insert fails
                        await _firebaseStorage.DeleteFileAsync(fileUrl);
                        return null;
                    }

                    images.Add(setImage);
                }
                catch (Exception)
                {
                    // Cleanup any successfully uploaded files
                    foreach (var successfulImage in images)
                    {
                        await _firebaseStorage.DeleteFileAsync(successfulImage.Url);
                    }
                    throw;
                }
            }
            return images;
        }

        public async Task<(long, Image?)> UpdateAnImage(Guid? id, FileModel model)
        {
            var existed = await _repository.ImageRepository.GetByID(id);
            if (existed == null)
            {
                return (1, null); //không tồn tại
            }
            if (existed.IsDeleted)
            {
                return (3, null);
            }

            try
            {
                // Upload new file
                var newFileUrl = await _firebaseStorage.UploadFileAsync(model.File);
                
                // Store old URL for cleanup
                var oldFileUrl = existed.Url;

                // Update entity
                existed.Url = newFileUrl;
                existed.Type = model.Type;
                existed.EntityId = model.EntityId;
                existed.AltText = model.AltText ?? model.File.FileName;
                existed = await SetBaseEntityToUpdateFunc(existed);

                var updateSuccess = await _repository.ImageRepository.Update(existed);
                if (updateSuccess)
                {
                    // Delete old file after successful update
                    await _firebaseStorage.DeleteFileAsync(oldFileUrl);
                    return (2, existed); //update thành công
                }

                // Cleanup new file if update fails
                await _firebaseStorage.DeleteFileAsync(newFileUrl);
                return (3, null); //update fail
            }
            catch
            {
                throw;
            }
        }

        public async Task<(long, Image?)> DeleteAnImage(Guid id)
        {
            var existed = await _repository.ImageRepository.GetByID(id);
            if (existed == null)
            {
                return (1, null); //không tồn tại
            }

            try
            {
                // Delete file from Firebase first
                await _firebaseStorage.DeleteFileAsync(existed.Url);

                existed.Type = null;
                existed.EntityId = null;
                existed = await SetBaseEntityToUpdateFunc(existed);

                return await _repository.ImageRepository.Delete(existed) 
                    ? (2, existed) //delete thành công
                    : (3, null);   //delete fail
            }
            catch
            {
                throw;
            }
        }

        public async Task<Image?> GetAnImageById(Guid id)
        {
            return await _repository.ImageRepository.GetByID(id);
        }

        public async Task<List<Image>?> GetImagesByTypeAndEntityID(string? type, Guid? entityID)
        {
            return await _repository.ImageRepository.GetByTypeAndEntityID(type, entityID);
        }

        public async Task<List<Image>?> GetAllImages()
        {
            try
            {
                var query = _repository.ImageRepository.GetQueryable();
                return await query.Where(x => !x.IsDeleted).ToListAsync();
            }
            catch
            {
                return null;
            }
        }
    }
}
