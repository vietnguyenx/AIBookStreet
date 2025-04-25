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
    public class EventService(IUnitOfWork repository, IMapper mapper, IHttpContextAccessor httpContextAccessor, IFirebaseStorageService firebaseStorageService, IImageService imageService) : BaseService<Event>(mapper, repository, httpContextAccessor), IEventService
    {
        private readonly IUnitOfWork _repository = repository;
        private readonly IFirebaseStorageService _firebaseStorageService = firebaseStorageService;
        private readonly IImageService _imageService = imageService;
        public async Task<(long, Event?)> AddAnEvent(EventModel model)
        {
            var existed = await _repository.EventRepository.GetAllPagination(null, null, model.StartDate, model.EndDate, model.ZoneId, 1, 1, null, true);
            if (existed.Item2 > 0 && existed.Item1 != null)
            {
                return (1, null); //da co su kien tren duong sach vao thoi gian nay
            }

            try
            {
                var baseFileUrl = "";
                if (model.BaseImgFile != null)
                {
                    baseFileUrl = await _firebaseStorageService.UploadFileAsync(model.BaseImgFile);
                }
                var videoUrl = "";
                if (model.VideoFile != null)
                {
                    videoUrl = await _firebaseStorageService.UploadFileAsync(model.VideoFile);
                }

                var evt = new Event
                {
                    BaseImgUrl = !string.IsNullOrEmpty(baseFileUrl) ? baseFileUrl : null,
                    EventName = model.EventName,
                    Description = model.Description ?? null,
                    StartDate = model.StartDate ?? null,
                    EndDate = model.EndDate ?? null,
                    VideoLink = !string.IsNullOrEmpty(videoUrl) ? videoUrl : null,
                    IsOpen = model.IsOpen,
                    ZoneId = model.ZoneId ?? null,
                };

                var setEvent = await SetBaseEntityToCreateFunc(evt);
                var isSuccess = await _repository.EventRepository.Add(setEvent);

                if (!isSuccess)
                {
                    if (model.BaseImgFile != null)
                    {
                        await _firebaseStorageService.DeleteFileAsync(baseFileUrl);
                    }
                    if (model.VideoFile != null)
                    {
                        await _firebaseStorageService.DeleteFileAsync(videoUrl);
                    }
                    return (3, null);
                }

                if (model.OtherImgFile != null)
                {
                    var images = new List<FileModel>();
                    foreach (var imageFile in model.OtherImgFile)
                    {
                        var image = new FileModel
                        {
                            File = imageFile,
                            Type = "Sự kiện",
                            AltText = "Ảnh sự kiện " + setEvent.EventName,
                            EntityId = setEvent.Id
                        };
                        images.Add(image);
                    }
                    var result = _imageService.AddImages(images);
                    if (result == null)
                    {
                        return (3, null);
                    }
                }
                return (2, setEvent);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<(long, Event?)> UpdateAnEvent(Guid id, EventModel model)
        {
            var existed = await _repository.EventRepository.GetByID(id);
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
                var newVideoUrl = model.VideoFile != null ? await _firebaseStorageService.UploadFileAsync(model.VideoFile) : "";

                var oldFileUrl = existed.BaseImgUrl;
                var oldVideoFile = existed.VideoLink;
                // Update entity
                existed.EventName = model.EventName;
                existed.Description = model.Description != null ? model.Description : existed.Description;
                existed.StartDate = model.StartDate != null ? model.StartDate.Value.ToLocalTime() : existed.StartDate;
                existed.EndDate = model.EndDate != null ? model.EndDate.Value.ToLocalTime() : existed.EndDate;
                existed.BaseImgUrl = newFileUrl == "" ? existed.BaseImgUrl : newFileUrl;
                existed.VideoLink = newVideoUrl == "" ? existed.VideoLink : newVideoUrl;
                existed.IsOpen = model.IsOpen;
                existed.ZoneId = model.ZoneId != null ? model.ZoneId : existed.ZoneId;
                existed = await SetBaseEntityToUpdateFunc(existed);

                var updateSuccess = await _repository.EventRepository.Update(existed);
                if (updateSuccess)
                {
                    if (model.BaseImgFile != null && !string.IsNullOrEmpty(oldFileUrl))
                    {
                        await _firebaseStorageService.DeleteFileAsync(oldFileUrl);
                    }
                    if (model.VideoFile != null && !string.IsNullOrEmpty(oldVideoFile))
                    {
                        await _firebaseStorageService.DeleteFileAsync(oldVideoFile);
                    }
                    return (2, existed); //update thành công
                }

                // Cleanup new file if update fails
                if (model.BaseImgFile != null)
                {
                    await _firebaseStorageService.DeleteFileAsync(newFileUrl);
                }
                if (model.VideoFile != null)
                {
                    await _firebaseStorageService.DeleteFileAsync(newVideoUrl);
                }
                return (3, null); //update fail
            }
            catch
            {
                throw;
            }
        }
        public async Task<(long, Event?)> DeleteAnEvent(Guid id)
        {
            var existed = await _repository.EventRepository.GetByID(id);
            if (existed == null)
            {
                return (1, null); //khong ton tai
            }
            if (existed.IsDeleted)
            {
                return (3, null);
            }
            existed = await SetBaseEntityToUpdateFunc(existed);

            var isSuccess = await _repository.EventRepository.Delete(existed);
            if (isSuccess)
            {
                try
                {
                    if (!string.IsNullOrEmpty(existed.BaseImgUrl))
                    {
                        await _firebaseStorageService.DeleteFileAsync(existed.BaseImgUrl);
                    }
                    if (!string.IsNullOrEmpty(existed.VideoLink))
                    {
                        await _firebaseStorageService.DeleteFileAsync(existed.VideoLink);
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
        public async Task<(Event?, List<object>, List<object>, List<object>, List<object>, List<object>, int)> GetAnEventById(Guid id)
        {
            var evt = await _repository.EventRepository.GetByID(id);
            var statistic = await _repository.EventRegistrationRepository.GetStatistic(id);
            return (evt, statistic.Item1, statistic.Item2, statistic.Item3, statistic.Item4, statistic.Item5, statistic.Item6);
        }
        public async Task<(List<Event>?, long)> GetAllEventsPagination(string? key, bool? allowAds, DateTime? start, DateTime? end, Guid? streetID, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var user = await GetUserInfo();
            var isAdmin = false;
            if (user != null)
            {
                foreach (var userRole in user.UserRoles)
                {
                    if (userRole.Role.RoleName == "Quản trị viên")
                    {
                        isAdmin = true;
                    }
                }
            }
            var events = isAdmin ? await _repository.EventRepository.GetAllPaginationForAdmin(key, allowAds, start, end, streetID, pageNumber, pageSize, sortField, desc)
                                                   : await _repository.EventRepository.GetAllPagination(key, allowAds, start, end, streetID, pageNumber, pageSize, sortField, desc);
            return events.Item1.Count > 0 ? (events.Item1, events.Item2) : (null, 0);
        }
        public async Task<List<Event>?> GetEventComing(int number, bool? allowAds)
        {
            var events = await _repository.EventRepository.GetEventsComing(number, allowAds);
            return events;
        }
        public async Task<List<DateModel>?> GetEventDatesInMonth(int? month)
        {
            var result = await _repository.EventRepository.GetDatesInMonth(month);
            var dates = new List<DateModel>();
            if (result != null)
            {
                foreach (var date in result)
                {
                    dates.Add(new DateModel(date));
                }
            }
            return dates;
        }
        public async Task<List<Event>?> GetEventByDate(DateTime? date)
        {
            return await _repository.EventRepository.GetByDate(date);
        }
        public async Task<List<Event>> GetRandom(int number)
        {
            return await _repository.EventRepository.GetRandom(number);
        }
        public async Task<object> GetNumberEventInMonth(int month)
        {
            return await _repository.EventRepository.GetNumberEventInMonth(month);
        }
    }
}
