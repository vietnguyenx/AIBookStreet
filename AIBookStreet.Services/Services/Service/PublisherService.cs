using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using AIBookStreet.Repositories.Repositories.Repositories.Repository;
using AIBookStreet.Repositories.Repositories.UnitOfWork.Interface;
using AIBookStreet.Services.Base;
using AIBookStreet.Services.Common;
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
    public class PublisherService : BaseService<Publisher>, IPublisherService
    {
        private readonly IPublisherRepository _publisherRepository;
        private readonly IImageService _imageService;

        public PublisherService(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, IImageService imageService) : base(mapper, unitOfWork, httpContextAccessor)
        {
            _publisherRepository = unitOfWork.PublisherRepository;
            _imageService = imageService;
        }

        public async Task<List<PublisherModel>> GetAll()
        {
            var publishers = await _publisherRepository.GetAll();

            if (!publishers.Any())
            {
                return null;
            }

            return _mapper.Map<List<PublisherModel>>(publishers);
        }

        public async Task<List<PublisherModel>> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var publishers = await _publisherRepository.GetAllPagination(pageNumber, pageSize, sortField, sortOrder);

            if (!publishers.Any())
            {
                return null;
            }

            return _mapper.Map<List<PublisherModel>>(publishers);
        }

        public async Task<PublisherModel?> GetById(Guid id)
        {
            var publisher = await _publisherRepository.GetById(id);

            if (publisher == null)
            {
                return null;
            }

            return _mapper.Map<PublisherModel>(publisher);
        }

        public async Task<(List<PublisherModel>?, long)> SearchPagination(PublisherModel publisherModel, int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var publishers = _mapper.Map<Publisher>(publisherModel);
            var publishersWithTotalOrigin = await _publisherRepository.SearchPagination(publishers, pageNumber, pageSize, sortField, sortOrder);

            if (!publishersWithTotalOrigin.Item1.Any())
            {
                return (null, publishersWithTotalOrigin.Item2);
            }
            var publisherModels = _mapper.Map<List<PublisherModel>>(publishersWithTotalOrigin.Item1);

            return (publisherModels, publishersWithTotalOrigin.Item2);
        }

        public async Task<List<PublisherModel>?> SearchWithoutPagination(PublisherModel publisherModel)
        {
            var publisher = _mapper.Map<Publisher>(publisherModel);
            var publishers = await _publisherRepository.SearchWithoutPagination(publisher);

            if (!publishers.Any())
            {
                return null;
            }

            var publisherModels = _mapper.Map<List<PublisherModel>>(publishers);
            return publisherModels;
        }

        public async Task<(PublisherModel?, string)> Add(PublisherModel publisherModel)
        {
            try
            {
                if (publisherModel == null)
                    return (null, ConstantMessage.Common.EmptyInfo);

                if (string.IsNullOrEmpty(publisherModel.PublisherName))
                    return (null, ConstantMessage.Publisher.EmptyPublisherName);

                var existingPublisher = await _publisherRepository.SearchWithoutPagination(new Publisher { PublisherName = publisherModel.PublisherName });
                if (existingPublisher?.Any() == true)
                    return (null, ConstantMessage.Publisher.PublisherNameExists);

                var mappedPublisher = _mapper.Map<Publisher>(publisherModel);
                var newPublisher = await SetBaseEntityToCreateFunc(mappedPublisher);            

                if (publisherModel.MainImageFile != null)
                {
                    if (publisherModel.MainImageFile.Length > 10 * 1024 * 1024)
                        return (null, ConstantMessage.Image.MainImageSizeExceeded);

                    if (!publisherModel.MainImageFile.ContentType.StartsWith("image/"))
                        return (null, ConstantMessage.Image.InvalidMainImageFormat);

                    var mainImageModel = new FileModel
                    {
                        File = publisherModel.MainImageFile,
                        Type = "publisher_main",
                        AltText = publisherModel.PublisherName ?? publisherModel.MainImageFile.FileName,
                        EntityId = newPublisher.Id
                    };

                    var mainImages = await _imageService.AddImages(new List<FileModel> { mainImageModel });
                    if (mainImages == null || !mainImages.Any())
                        return (null, ConstantMessage.Image.MainImageUploadFailed);

                    newPublisher.BaseImgUrl = mainImages.First().Url;
                }

                if (publisherModel.AdditionalImageFiles?.Any() == true)
                {
                    foreach (var file in publisherModel.AdditionalImageFiles)
                    {
                        if (file.Length > 10 * 1024 * 1024)
                            return (null, ConstantMessage.Image.SubImageSizeExceeded);

                        if (!file.ContentType.StartsWith("image/"))
                            return (null, ConstantMessage.Image.InvalidSubImageFormat);
                    }

                    var additionalImageModels = publisherModel.AdditionalImageFiles.Select(file => new FileModel
                    {
                        File = file,
                        Type = "publisher_additional",
                        AltText = publisherModel.PublisherName ?? file.FileName,
                        EntityId = newPublisher.Id
                    }).ToList();

                    var additionalImages = await _imageService.AddImages(additionalImageModels);
                    if (additionalImages == null)
                        return (null, ConstantMessage.Image.SubImageUploadFailed);
                }

                var result = await _publisherRepository.Add(newPublisher);
                if (!result)
                    return (null, ConstantMessage.Common.AddFail);

                return (_mapper.Map<PublisherModel>(newPublisher), ConstantMessage.Common.AddSuccess);
            }
            catch (Exception ex)
            {
                return (null, $"Error while adding publisher: {ex.Message}");
            }
        }

        public async Task<(PublisherModel?, string)> Update(PublisherModel publisherModel)
        {
            try
            {
                if (publisherModel == null)
                    return (null, ConstantMessage.Common.EmptyInfo);

                if (publisherModel.Id == Guid.Empty)
                    return (null, ConstantMessage.EmptyId);

                var existingPublisher = await _publisherRepository.GetById(publisherModel.Id);
                if (existingPublisher == null)
                    return (null, ConstantMessage.Common.NotFoundForUpdate);

                if (!string.IsNullOrEmpty(publisherModel.PublisherName) && publisherModel.PublisherName != existingPublisher.PublisherName)
                {
                    var PubilsherWithSameName = await _publisherRepository.SearchWithoutPagination(new Publisher { PublisherName = publisherModel.PublisherName });
                    if (PubilsherWithSameName?.Any() == true)
                        return (null, ConstantMessage.Publisher.PublisherNameExists);
                }

                if (string.IsNullOrEmpty(publisherModel.PublisherName))
                    publisherModel.PublisherName = existingPublisher.PublisherName;

                _mapper.Map(publisherModel, existingPublisher);
                var updatedPublisher = await SetBaseEntityToUpdateFunc(existingPublisher);

                if (publisherModel.MainImageFile != null)
                {
                    if (publisherModel.MainImageFile.Length > 10 * 1024 * 1024)
                        return (null, ConstantMessage.Image.MainImageSizeExceeded);

                    if (!publisherModel.MainImageFile.ContentType.StartsWith("image/"))
                        return (null, ConstantMessage.Image.InvalidMainImageFormat);

                    var existingMainImages = await _imageService.GetImagesByTypeAndEntityID("publisher_main", updatedPublisher.Id);
                    if (existingMainImages?.Any() == true)
                    {
                        var mainImageModel = new FileModel
                        {
                            File = publisherModel.MainImageFile,
                            Type = "publisher_main",
                            AltText = publisherModel.PublisherName ?? publisherModel.MainImageFile.FileName,
                            EntityId = updatedPublisher.Id
                        };

                        var updateResult = await _imageService.UpdateAnImage(existingMainImages.First().Id, mainImageModel);
                        if (updateResult.Item1 != 2)
                            return (null, ConstantMessage.Image.MainImageUploadFailed);

                        updatedPublisher.BaseImgUrl = updateResult.Item2.Url;
                    }
                    else
                    {
                        var mainImageModel = new FileModel
                        {
                            File = publisherModel.MainImageFile,
                            Type = "publisher_main",
                            AltText = publisherModel.PublisherName ?? publisherModel.MainImageFile.FileName,
                            EntityId = updatedPublisher.Id
                        };

                        var mainImages = await _imageService.AddImages(new List<FileModel> { mainImageModel });
                        if (mainImages == null)
                            return (null, ConstantMessage.Image.MainImageUploadFailed);

                        updatedPublisher.BaseImgUrl = mainImages.First().Url;
                    }
                }

                if (publisherModel.AdditionalImageFiles?.Any() == true)
                {
                    foreach (var file in publisherModel.AdditionalImageFiles)
                    {
                        if (file.Length > 10 * 1024 * 1024)
                            return (null, ConstantMessage.Image.SubImageSizeExceeded);

                        if (!file.ContentType.StartsWith("image/"))
                            return (null, ConstantMessage.Image.InvalidSubImageFormat);
                    }

                    var existingAdditionalImages = await _imageService.GetImagesByTypeAndEntityID("publisher_additional", updatedPublisher.Id);
                    if (existingAdditionalImages?.Any() == true)
                    {
                        foreach (var image in existingAdditionalImages)
                        {
                            var deleteResult = await _imageService.DeleteAnImage(image.Id);
                            if (deleteResult.Item1 != 2)
                                return (null, ConstantMessage.Image.SubImageUploadFailed);
                        }
                    }

                    var additionalImageModels = publisherModel.AdditionalImageFiles.Select(file => new FileModel
                    {
                        File = file,
                        Type = "publisher_additional",
                        AltText = publisherModel.PublisherName ?? file.FileName,
                        EntityId = updatedPublisher.Id
                    }).ToList();

                    var additionalImages = await _imageService.AddImages(additionalImageModels);
                    if (additionalImages == null)
                        return (null, ConstantMessage.Image.SubImageUploadFailed);
                }

                var result = await _publisherRepository.Update(updatedPublisher);
                if (!result)
                    return (null, ConstantMessage.Common.UpdateFail);

                return (_mapper.Map<PublisherModel>(updatedPublisher), ConstantMessage.Common.UpdateSuccess);
            }
            catch (Exception ex)
            {
                return (null, $"Error while updating publisher: {ex.Message}");
            }
        }

        public async Task<(PublisherModel?, string)> Delete(Guid publisherId)
        {
            try
            {
                if (publisherId == Guid.Empty)
                    return (null, ConstantMessage.EmptyId);

                var existingPublisher = await _publisherRepository.GetById(publisherId);
                if (existingPublisher == null)
                    return (null, ConstantMessage.Common.NotFoundForDelete);

                var existingMainImages = await _imageService.GetImagesByTypeAndEntityID("publisher_main", publisherId);
                var existingAdditionalImages = await _imageService.GetImagesByTypeAndEntityID("publisher_additional", publisherId);

                if (existingMainImages != null)
                {
                    foreach (var image in existingMainImages)
                    {
                        var deleteResult = await _imageService.DeleteAnImage(image.Id);
                        if (deleteResult.Item1 != 2)
                            return (null, ConstantMessage.Image.MainImageUploadFailed);
                    }
                }

                if (existingAdditionalImages != null)
                {
                    foreach (var image in existingAdditionalImages)
                    {
                        var deleteResult = await _imageService.DeleteAnImage(image.Id);
                        if (deleteResult.Item1 != 2)
                            return (null, ConstantMessage.Image.SubImageUploadFailed);
                    }
                }

                var result = await _publisherRepository.Delete(existingPublisher);
                if (!result)
                    return (null, ConstantMessage.Common.DeleteFail);

                return (_mapper.Map<PublisherModel>(existingPublisher), ConstantMessage.Common.DeleteSuccess);
            }
            catch (Exception ex)
            {
                return (null, $"Error while deleting publisher: {ex.Message}");
            }
        }


    }
}
