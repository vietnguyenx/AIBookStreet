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
    public class StoreService : BaseService<Store>, IStoreService
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IImageService _imageService;

        public StoreService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor, IImageService imageService) : base(mapper, unitOfWork, httpContextAccessor)
        {
            _storeRepository = unitOfWork.StoreRepository;
            _imageService = imageService;
        }

        public async Task<List<StoreModel>> GetAll()
        {
            var stores = await _storeRepository.GetAll();

            if (!stores.Any())
            {
                return null;
            }

            return _mapper.Map<List<StoreModel>>(stores);
        }

        public async Task<List<StoreModel>?> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var stores = await _storeRepository.GetAllPagination(pageNumber, pageSize, sortField, sortOrder);

            if (!stores.Any())
            {
                return null;
            }

            return _mapper.Map<List<StoreModel>>(stores);
        }

        public async Task<StoreModel?> GetById(Guid id)
        {
            var store = await _storeRepository.GetById(id);

            if (store == null)
            {
                return null;
            }

            return _mapper.Map<StoreModel>(store);
        }

        public async Task<(List<StoreModel>?, long)> SearchPagination(StoreModel storeModel, int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var stores = _mapper.Map<Store>(storeModel);
            var storesWithTotalOrigin = await _storeRepository.SearchPagination(stores, pageNumber, pageSize, sortField, sortOrder);

            if (!storesWithTotalOrigin.Item1.Any())
            {
                return (null, storesWithTotalOrigin.Item2);
            }
            var storeModels = _mapper.Map<List<StoreModel>>(storesWithTotalOrigin.Item1);

            return (storeModels, storesWithTotalOrigin.Item2);
        }

        public async Task<List<StoreModel>?> SearchWithoutPagination(StoreModel storeModel)
        {
            var store = _mapper.Map<Store>(storeModel);
            var stores = await _storeRepository.SearchWithoutPagination(store);

            if (!stores.Any())
            {
                return null;
            }

            return _mapper.Map<List<StoreModel>>(stores);
        }


        public async Task<(StoreModel?, string)> Add(StoreModel storeModel)
        {
            try
            {
                if (storeModel == null)
                    return (null, ConstantMessage.Common.EmptyInfo);

                if (string.IsNullOrEmpty(storeModel.StoreName))
                    return (null, ConstantMessage.Store.EmptyStoreName);

                var existingStore = await _storeRepository.SearchWithoutPagination(new Store { StoreName = storeModel.StoreName });
                if (existingStore?.Any() == true)
                    return (null, ConstantMessage.Store.StoreNameExists);

                var mappedStore = _mapper.Map<Store>(storeModel);
                var newStore = await SetBaseEntityToCreateFunc(mappedStore);

                if (storeModel.MainImageFile != null)
                {
                    if (storeModel.MainImageFile.Length > 10 * 1024 * 1024)
                        return (null, ConstantMessage.Image.MainImageSizeExceeded);

                    if (!storeModel.MainImageFile.ContentType.StartsWith("image/"))
                        return (null, ConstantMessage.Image.InvalidMainImageFormat);

                    var mainImageModel = new FileModel
                    {
                        File = storeModel.MainImageFile,
                        Type = "store_main",
                        AltText = storeModel.StoreName ?? storeModel.MainImageFile.Name,
                        EntityId = newStore.Id
                    };

                    var mainImages = await _imageService.AddImages(new List<FileModel> { mainImageModel });
                    if (mainImages == null || !mainImages.Any())
                        return (null, ConstantMessage.Image.MainImageUploadFailed);

                    newStore.BaseImgUrl = mainImages.First().Url;
                }

                if (storeModel.AdditionalImageFiles?.Any() == true)
                {
                    foreach (var file in storeModel.AdditionalImageFiles)
                    {
                        if (file.Length > 10 * 1024 * 1024)
                            return (null, ConstantMessage.Image.SubImageSizeExceeded);

                        if (!file.ContentType.StartsWith("image/"))
                            return (null, ConstantMessage.Image.InvalidSubImageFormat);
                    }

                    var additionalImageModels = storeModel.AdditionalImageFiles.Select(file => new FileModel
                    {
                        File = file,
                        Type = "store_additional",
                        AltText = storeModel.StoreName ?? file.FileName,
                        EntityId = newStore.Id
                    }).ToList();

                    var additionalImages = await _imageService.AddImages(additionalImageModels);
                    if (additionalImages == null)
                        return (null, ConstantMessage.Image.SubImageUploadFailed);
                }

                var result = await _storeRepository.Add(newStore);
                if (!result)
                    return (null, ConstantMessage.Common.AddFail);

                return (_mapper.Map<StoreModel>(newStore), ConstantMessage.Common.AddSuccess);
            }
            catch (Exception ex)
            {
                return (null, $"Error while adding store: {ex.Message}");
            }
        }

        public async Task<(StoreModel?, string)> Update(StoreModel storeModel)
        {
            try
            {
                if (storeModel == null)
                    return (null, ConstantMessage.Common.EmptyInfo);

                if (storeModel.Id == Guid.Empty)
                    return (null, ConstantMessage.EmptyId);

                var existingStore = await _storeRepository.GetById(storeModel.Id);
                if (existingStore == null)
                    return (null, ConstantMessage.Common.NotFoundForUpdate);

                if (!string.IsNullOrEmpty(storeModel.StoreName) && storeModel.StoreName != existingStore.StoreName)
                {
                    var storeWithSameName = await _storeRepository.SearchWithoutPagination(new Store { StoreName = storeModel.StoreName });
                    if (storeWithSameName?.Any() == true)
                        return (null, ConstantMessage.Store.StoreNameExists);
                }

                if (string.IsNullOrEmpty(storeModel.StoreName))
                    storeModel.StoreName = existingStore.StoreName;

                _mapper.Map(storeModel, existingStore);
                var updatedStore = await SetBaseEntityToUpdateFunc(existingStore);

                if (storeModel.MainImageFile != null)
                {
                    if (storeModel.MainImageFile.Length > 10 * 1024 * 1024)
                        return (null, ConstantMessage.Image.MainImageSizeExceeded);

                    if (!storeModel.MainImageFile.ContentType.StartsWith("image/"))
                        return (null, ConstantMessage.Image.InvalidMainImageFormat);

                    var existingMainImages = await _imageService.GetImagesByTypeAndEntityID("store_main", updatedStore.Id);
                    if (existingMainImages?.Any() == true)
                    {
                        var mainImageModel = new FileModel
                        {
                            File = storeModel.MainImageFile,
                            Type = "store_main",
                            AltText = storeModel.StoreName ?? storeModel.MainImageFile.FileName,
                            EntityId = updatedStore.Id
                        };

                        var updateResult = await _imageService.UpdateAnImage(existingMainImages.First().Id, mainImageModel);
                        if (updateResult.Item1 != 2)
                            return (null, ConstantMessage.Image.MainImageUploadFailed);

                        updatedStore.BaseImgUrl = updateResult.Item2.Url;
                    }
                    else
                    {
                        var mainImageModel = new FileModel
                        {
                            File = storeModel.MainImageFile,
                            Type = "store_main",
                            AltText = storeModel.StoreName ?? storeModel.MainImageFile.FileName,
                            EntityId = updatedStore.Id
                        };

                        var mainImages = await _imageService.AddImages(new List<FileModel> { mainImageModel });
                        if (mainImages == null)
                            return (null, ConstantMessage.Image.MainImageUploadFailed);

                        updatedStore.BaseImgUrl = mainImages.First().Url;
                    }
                }

                if (storeModel.AdditionalImageFiles?.Any() == true)
                {
                    foreach (var file in storeModel.AdditionalImageFiles)
                    {
                        if (file.Length > 10 * 1024 * 1024)
                            return (null, ConstantMessage.Image.SubImageSizeExceeded);

                        if (!file.ContentType.StartsWith("image/"))
                            return (null, ConstantMessage.Image.InvalidSubImageFormat);
                    }

                    var existingAdditionalImages = await _imageService.GetImagesByTypeAndEntityID("store_additional", updatedStore.Id);
                    if (existingAdditionalImages?.Any() == true)
                    {
                        foreach (var image in existingAdditionalImages)
                        {
                            var deleteResult = await _imageService.DeleteAnImage(image.Id);
                            if (deleteResult.Item1 != 2)
                                return (null, ConstantMessage.Image.SubImageUploadFailed);
                        }
                    }

                    var additionalImageModels = storeModel.AdditionalImageFiles.Select(file => new FileModel
                    {
                        File = file,
                        Type = "store_additional",
                        AltText = storeModel.StoreName ?? file.FileName,
                        EntityId = updatedStore.Id
                    }).ToList();

                    var additionalImages = await _imageService.AddImages(additionalImageModels);
                    if (additionalImages == null)
                        return (null, ConstantMessage.Image.SubImageUploadFailed);
                }

                var result = await _storeRepository.Update(updatedStore);
                if (!result)
                    return (null, ConstantMessage.Common.UpdateFail);

                return (_mapper.Map<StoreModel>(updatedStore), ConstantMessage.Common.UpdateSuccess);
            }
            catch (Exception ex)
            {
                return (null, $"Error while updating store: {ex.Message}");
            }
        }

        public async Task<(StoreModel?, string)> Delete(Guid storeId)
        {
            try
            {
                if (storeId == Guid.Empty)
                    return (null, ConstantMessage.EmptyId);

                var existingStore = await _storeRepository.GetById(storeId);
                if (existingStore == null)
                    return (null, ConstantMessage.Common.NotFoundForDelete);

                var existingMainImages = await _imageService.GetImagesByTypeAndEntityID("store_main", storeId);
                var existingAdditionalImages = await _imageService.GetImagesByTypeAndEntityID("store_additional", storeId);

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

                var result = await _storeRepository.Delete(existingStore);
                if (!result)
                    return (null, ConstantMessage.Common.DeleteFail);

                return (_mapper.Map<StoreModel>(existingStore), ConstantMessage.Common.DeleteSuccess);
            }
            catch (Exception ex)
            {
                return (null, $"Error while deleting store: {ex.Message}");
            }
        }

        public async Task<long> GetTotalCount()
        {
            return await _storeRepository.GetTotalCount();
        }

        public async Task<(long totalCount, double percentChange)> GetTotalStoreCountWithChangePercent()
        {
            try
            {
                // Get total count of stores
                var totalCount = await GetTotalCount();
                
                // Get current month and year
                var currentDate = DateTime.Now;
                var currentMonth = currentDate.Month;
                var currentYear = currentDate.Year;
                
                // Get previous month and year
                var previousMonth = currentMonth == 1 ? 12 : currentMonth - 1;
                var previousYear = currentMonth == 1 ? currentYear - 1 : currentYear;
                
                // Get stores from current month
                var currentMonthStores = await _storeRepository.GetStoresByMonth(currentMonth, currentYear);
                var currentMonthCount = currentMonthStores.Count();
                
                // Get stores from previous month
                var previousMonthStores = await _storeRepository.GetStoresByMonth(previousMonth, previousYear);
                var previousMonthCount = previousMonthStores.Count();
                
                // Calculate percent change
                double percentChange = 0;
                if (previousMonthCount > 0)
                {
                    percentChange = Math.Round(((double)(currentMonthCount - previousMonthCount) / previousMonthCount) * 100, 2);
                }
                
                return (totalCount, percentChange);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
