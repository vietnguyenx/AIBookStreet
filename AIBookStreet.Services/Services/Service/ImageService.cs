using AIBookStreet.Repositories.Data.Entities;
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
    public class ImageService(IUnitOfWork repository, IMapper mapper, IHttpContextAccessor httpContextAccessor
        //, IConfiguration configuration, StorageClient storageClient
        ) : BaseService<Image>(mapper, repository, httpContextAccessor), IImageService
    {
        private readonly IUnitOfWork _repository = repository;
        //private readonly StorageClient _storageClient = storageClient;
        //private readonly string _bucketName =  _configuration["Firebase:Bucket"]!;
        //public string GetImageUrl(string imageName)
        //{

        //    string imageUrl = $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{Uri.EscapeDataString(imageName)}?alt=media";
        //    return imageUrl;
        //}
        //public async Task DeleteImageAsync(string imageName)
        //{
        //    await _storageClient.DeleteObjectAsync(_bucketName, imageName, cancellationToken: CancellationToken.None);
        //}
        //public async Task<List<string>?> UploadImagesAsync(List<IFormFile> files)  
        //{

        //    using var stream = new MemoryStream();

        //    var imageUrls = new List<string>();

        //    foreach ( var file in files )
        //    {
        //        await file.CopyToAsync(stream);

        //        var blob = await _storageClient.UploadObjectAsync(_bucketName, file.FileName, file.ContentType, stream, cancellationToken: CancellationToken.None);

        //        if (blob is null)
        //        {
        //            return null;
        //        }
        //        imageUrls.Add(GetImageUrl(file.FileName));
        //    }

        //    return imageUrls;
        //}
        public async Task<List<Image>?> AddImages(List<ImageModel> models)
        {
            var images = new List<Image>();
            foreach (var model in models)
            {
                var image = _mapper.Map<Image>(model);
                var setImage = await SetBaseEntityToCreateFunc(image);
                var isSuccess = await _repository.ImageRepository.Add(setImage);
                if (isSuccess)
                {
                    images.Add(setImage);
                }
                return null;
            }
            return images;
        }
        public async Task<(long, Image?)> UpdateAnImage(Guid? id, ImageModel model)
        {
            var existed = await _repository.ImageRepository.GetByID(id);
            if (existed == null)
            {
                return (1, null); //khong ton tai
            }
            if (existed.IsDeleted)
            {
                return (3, null);
            }
            existed.Url = model.Url;
            existed.Type = model.Type;
            existed.EntityId = model.EntityId;
            existed.AltText = model.AltText;
            existed = await SetBaseEntityToUpdateFunc(existed);
            return await _repository.ImageRepository.Update(existed) ? (2, existed) //update thanh cong
                                                                          : (3, null);       //update fail
        }
        public async Task<(long, Image?)> DeleteAnImage(Guid id)
        {
            var existed = await _repository.ImageRepository.GetByID(id);
            if (existed == null)
            {
                return (1, null); //khong ton tai
            }
            existed.Type = null;
            existed.EntityId = null;
            existed = await SetBaseEntityToUpdateFunc(existed);

            return await _repository.ImageRepository.Delete(existed) ? (2, existed) //delete thanh cong
                                                                          : (3, null);       //delete fail
        }
        public async Task<Image?> GetAnImageById(Guid id)
        {
            return await _repository.ImageRepository.GetByID(id);
        }
        public async Task<List<Image>?> GetImagesByTypeAndEntityID(string? type, Guid? entityID)
        {
            return await _repository.ImageRepository.GetByTypeAndEntityID(type, entityID);
        }
    }
}
