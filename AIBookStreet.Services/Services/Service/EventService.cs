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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AIBookStreet.Services.Services.Service
{
    public class EventService(IUnitOfWork repository, IMapper mapper, IHttpContextAccessor httpContextAccessor, IFirebaseStorageService firebaseStorageService, IImageService imageService) : BaseService<Event>(mapper, repository, httpContextAccessor), IEventService
    {
        private readonly IUnitOfWork _repository = repository;
        private readonly IFirebaseStorageService _firebaseStorageService = firebaseStorageService;
        private readonly IImageService _imageService = imageService;
        public async Task<(long, Event?, string?)> AddAnEvent(EventModel model, List<EventScheduleModel> schedules)
        {         
            try
            {
                var user = await GetUserInfo();
                var isStaff = false;
                var isAdmin = false;
                if (user != null)
                {
                    foreach (var userRole in user.UserRoles)
                    {
                        if (userRole.Role.RoleName == "Organizer" || userRole.Role.RoleName == "Admin")
                        {
                            isStaff = true;
                            if (userRole.Role.RoleName == "Admin")
                            {
                                isAdmin = true;
                            }
                        }
                    }
                }
                if (!isStaff)
                {
                    return (4, null, "Vui lòng đăng nhập với vai trò 'Người tổ chức sự kiện' hoặc 'Quản trị viên'");
                }
                var existed = await _repository.EventRepository.CheckEventInZone(schedules.OrderBy(e => e.EventDate).FirstOrDefault()?.EventDate, schedules.OrderBy(e => e.EventDate).LastOrDefault()?.EventDate, model.ZoneId);
                //var existed = await _repository.EventRepository.CheckEventInZone(model.EventScheduleModel.EventDate, model.EventScheduleModel.EventDate, model.ZoneId);
                if (existed != null)
                {
                    return (1, null, existed); //da co su kien tren duong sach vao thoi gian nay
                }
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

                var lastEventByOrganizerEmail = await _repository.EventRepository.GetLastEventByOrganizerEmail(user.Email);
                if (lastEventByOrganizerEmail != null && !lastEventByOrganizerEmail.IsApprove.HasValue)
                {
                    return (3, null, "Sự kiện đang chờ duyệt, không thể tạo thêm");
                }

                var version = 1;
                if (lastEventByOrganizerEmail != null && lastEventByOrganizerEmail.IsApprove.HasValue && lastEventByOrganizerEmail.IsApprove == false)
                {
                    version = lastEventByOrganizerEmail.Version + 1;
                }

                var evt = new Event
                {
                    BaseImgUrl = !string.IsNullOrEmpty(baseFileUrl) ? baseFileUrl : null,
                    EventName = model.EventName,
                    Description = model.Description ?? null,
                    VideoLink = !string.IsNullOrEmpty(videoUrl) ? videoUrl : null,
                    IsOpen = model.IsOpen,
                    AllowAds = model.AllowAds,
                    ZoneId = model.ZoneId,
                    OrganizerEmail = user.Email,
                    IsApprove = isAdmin ? true : null,
                    Message = null,
                    Version = isAdmin ? 1 : version,
                    UpdateForEventId = isAdmin ? null : version == 1 ? null : version == 2 ? lastEventByOrganizerEmail?.Id : lastEventByOrganizerEmail?.UpdateForEventId
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
                    return (3, null, "Thêm thất bại, vui lòng kiểm tra lại");
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
                        await _repository.EventRepository.Remove(setEvent);
                        return (3, null, "Thêm ảnh thất bại, vui lòng kiểm tra lại");
                    }
                }
                if (version != 1 && lastEventByOrganizerEmail != null && lastEventByOrganizerEmail.EventSchedules != null)
                {
                    foreach(var schedule in lastEventByOrganizerEmail.EventSchedules)
                    {
                        await _repository.EventScheduleRepository.Remove(schedule);
                    }
                }
                foreach (var schedule in schedules)
                {
                    var evtSchedule = new EventSchedule
                    {
                        Id = Guid.NewGuid(),
                        CreatedBy = user.Email,
                        CreatedDate = DateTime.Now,
                        LastUpdatedBy = user.Email,
                        LastUpdatedDate = DateTime.Now,
                        IsDeleted = false,
                        EventDate = DateOnly.Parse(schedule.EventDate),
                        StartTime = TimeOnly.Parse(schedule.StartTime),
                        EndTime = TimeOnly.Parse(schedule.EndTime),
                        EventId = setEvent.Id
                    };
                    var res = await _repository.EventScheduleRepository.Add(evtSchedule);
                    if (!res)
                    {
                        await _repository.EventRepository.Remove(setEvent);
                        return (3, null, "Thêm lịch sự kiện thất bại, vui lòng kiểm tra lại");
                    }
                }
                //var evtSchedule = new EventSchedule
                //{
                //    Id = Guid.NewGuid(),
                //    CreatedBy = user.Email,
                //    CreatedDate = DateTime.Now,
                //    LastUpdatedBy = user.Email,
                //    LastUpdatedDate = DateTime.Now,
                //    IsDeleted = false,
                //    EventDate = DateOnly.Parse(model.EventScheduleModel.EventDate),
                //    StartTime = TimeOnly.Parse(model.EventScheduleModel.StartTime),
                //    EndTime = TimeOnly.Parse(model.EventScheduleModel.EndTime),
                //    EventId = setEvent.Id
                //};
                //var res = await _repository.EventScheduleRepository.Add(evtSchedule);
                //if (!res)
                //{
                //    await _repository.EventRepository.Remove(setEvent);
                //    return (3, null, "Thêm lịch sự kiện thất bại, vui lòng kiểm tra lại");
                //}

                return (2, setEvent, "Đã thêm thông tin sự kiện");
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<(long, Event?, string)> ProcessEvent(Guid id, ProcesingEventModel model)
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
                if (!isAdmin)
                {
                    return (4, null, "Vui lòng đăng nhập với vai trò Quản trị viên");
                }
                var existed = await _repository.EventRepository.GetByID(id);
                if (existed == null)
                {
                    return (1, null, "Không tìm thấy sự kiện"); //khong ton tai
                }
                if (existed.IsDeleted)
                {
                    return (3, null, "Sự kiện đã bị xóa");
                }
                
                if (!model.IsApprove && string.IsNullOrEmpty(model.Message))
                {
                    return (3, null, "Vui lòng nhập lý do từ chối sự kiện");
                }
                existed.IsApprove = model.IsApprove;
                existed.Message = !model.IsApprove ? model.Message : null;

                existed = await SetBaseEntityToUpdateFunc(existed);

                var updateSuccess = await _repository.EventRepository.Update(existed);
                if (updateSuccess)
                {
                    return (2, existed, "Đã xử lý sự kiện"); //update thành công
                }
                return (3, null, "Đã xảy ra lỗi, vui lòng thử lại sau"); //update fail
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<(long, Event?, string?)> DeleteAnEvent(Guid id)
        {
            try
            {
                var user = await GetUserInfo();
                var isOrganizer = false;
                if (user != null)
                {
                    foreach (var userRole in user.UserRoles)
                    {
                        if (userRole.Role.RoleName == "Organizer")
                        {
                            isOrganizer = true;
                        }
                    }
                }
                if (!isOrganizer)
                {
                    return (4, null, "Vui lòng đăng nhập với vai trò Người tổ chức sự kiện");
                }
                var existed = await _repository.EventRepository.GetByID(id);
                if (existed == null)
                {
                    return (1, null, "Không tìm thấy sự kiện"); //khong ton tai
                }
                if (existed.IsDeleted)
                {
                    return (3, null, "Sự kiện đã bị xóa trước đây");
                }
                existed = await SetBaseEntityToUpdateFunc(existed);

                var isSuccess = await _repository.EventRepository.Delete(existed);
                if (isSuccess)
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
                    var schedules = existed.EventSchedules;
                    if (schedules != null)
                    {
                        foreach (var schedule in schedules)
                        {
                            await _repository.EventScheduleRepository.Remove(schedule);
                        }
                    }
                    return (2, existed, "Đã xóa sự kiện");//delete thanh cong
                }
                return (3, null, "Không thể xóa sự kiện, vui lòng thử lại sau");       //delete fail
            } catch
            {
                throw;
            }
        }
        public async Task<(Event?, int)> GetAnEventById(Guid id)
        {
            var evt = await _repository.EventRepository.GetByID(id);
            evt.EventSchedules = evt.EventSchedules?.OrderBy(e => e.EventDate).ToList();
            var statistic = await _repository.EventRegistrationRepository.GetStatistic(id, null, null, null, null);
            return (evt, statistic.Item6);
        }
        public async Task<(List<Event>?, long)> GetAllEventsPagination(string? key, bool? allowAds, DateTime? start, DateTime? end, Guid? zoneId, int? pageNumber, int? pageSize, string? sortField, bool? desc)
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

            var events = isAdmin ? await _repository.EventRepository.GetAllPaginationForAdmin(key, allowAds, start, end, zoneId, pageNumber, pageSize, sortField, desc)
                                                   : await _repository.EventRepository.GetAllPagination(key, allowAds, zoneId, pageNumber, pageSize, sortField, desc);
            if (events.Item1?.Count > 0)
            {
                //foreach (var evt in events.Item1)
                //{
                //    if (evt.EventSchedules?.OrderByDescending(es => es.EventDate).FirstOrDefault()?.EventDate < DateOnly.FromDateTime(DateTime.Now))
                //    {
                //        evt.IsOpen = false;
                //        var updateSuccess = await _repository.EventRepository.Update(evt);
                //        if (!updateSuccess)
                //        {
                //            return (null, 0); //update thành công
                //        }
                //    }
                //}
                return (events.Item1, events.Item2);
            }
            return (null, 0);
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
        public async Task<List<Event>?> GetRandom(int number)
        {
            return await _repository.EventRepository.GetRandom(number);
        }
        public async Task<object> GetNumberEventInMonth(int month)
        {
            return await _repository.EventRepository.GetNumberEventInMonth(month);
        }
        public async Task<(List<Event>?, long)> GetEventsForCheckin(DateTime? date, string? eventName, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var user = await GetUserInfo();
            var isOrganizer = false;
            if (user != null)
            {
                foreach (var userRole in user.UserRoles)
                {
                    if (userRole.Role.RoleName == "Organizer")
                    {
                        isOrganizer = true;
                    }
                }
            }
            if (!isOrganizer)
            {
                return (null, 99);
            }
            var events = await _repository.EventRepository.GetEventsForOrganizer(date, eventName, user?.Email, pageNumber, pageSize, sortField, desc);
            return (events.Item1, events.Item2);
        }
        public async Task<(List<Event>?, long)> GetEventRequests(int? pageNumber, int? pageSize, string? sortField, bool? desc)
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
                if (!isAdmin)
                {
                    return (null, 99);
                }
                var events = await _repository.EventRepository.GetEventRequests(pageNumber, pageSize, sortField, desc);
                return (events.Item1, events.Item2);
            } catch
            {
                throw;
            }
        }
        public async Task<(long, Event?, string)> OpenState(Guid id)
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
                if (!isAdmin)
                {
                    return (4, null, "Vui lòng đăng nhập với vai trò Quản trị viên");
                }
                var existed = await _repository.EventRepository.GetByID(id);
                if (existed == null)
                {
                    return (1, null, "Không tìm thấy sự kiện"); //khong ton tai
                }
                if (existed.IsDeleted)
                {
                    return (3, null, "Sự kiện đã bị xóa");
                }

                existed.IsOpen = !existed.IsOpen;

                existed = await SetBaseEntityToUpdateFunc(existed);

                var updateSuccess = await _repository.EventRepository.Update(existed);
                if (updateSuccess)
                {
                    return (2, existed, "Đã xử lý sự kiện"); //update thành công
                }
                return (3, null, "Đã xảy ra lỗi, vui lòng thử lại sau"); //update fail
            } catch
            {
                throw;
            }
        }
        public async Task<(long, List<Event>?)> GetHistory(Guid eventId)
        {
            try
            {
                var existed = await _repository.EventRepository.GetByID(eventId);
                if (existed == null)
                {
                    return (1, null); //khong ton tai
                }
                var history = new List<Event>
                {
                    existed
                };
                var otherVersions = await _repository.EventRepository.GetHistory(existed.UpdateForEventId);
                if (otherVersions != null && otherVersions.Count > 0)
                {
                    otherVersions.Remove(otherVersions.Last());
                    foreach (var version in otherVersions)
                    {
                        history.Add(version);
                    }
                }
                return (2, history);
            }
            catch
            {
                throw;
            }
        }
        public async Task<(long, List<Event>?, string?)> GetCreationHistory(int? pageNumber, int? pageSize)
        {
            try
            {
                var user = await GetUserInfo();
                var isOrganizer = false;
                if (user != null)
                {
                    foreach (var userRole in user.UserRoles)
                    {
                        if (userRole.Role.RoleName == "Organizer")
                        {
                            isOrganizer = true;
                        }
                    }
                }
                if (!isOrganizer)
                {
                    return (0, null, "Vui lòng đăng nhập với vai trò Người tổ chức sự kiện");
                }
                var history = await _repository.EventRepository.GetCreationHistory(user?.Email, pageNumber, pageSize);
                return (1, history, null);
            } catch
            {
                throw;
            }
        }
    }
}
