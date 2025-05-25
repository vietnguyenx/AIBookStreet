using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IEventRegistrationService
    {
        Task<(long, List<EventRegistration>?,string)> AddAnEventRegistration(EventRegistrationModel model);
        Task<int> SendRegistrationEmai(Ticket? registration);
        Task<(long, List<EventRegistration>?, string)> CheckAttend(List<CheckAttendModel> models, Event? evt);
        //Task<(long, EventRegistration?)> DeleteAnEventRegistration(Guid id);
        Task<EventRegistration?> GetAnEventRegistrationById(Guid? id);
        Task<(long, List<EventRegistration>?)> GetAllActiveEventRegistrationsInAnEvent(Guid eventId, string? searchKey, string? date);
        Task<(List<object>, List<object>, List<object>, List<object>, List<object>, int, int)> GetAnEventStatistics(Guid eventId, bool? isAttended, string? province, string? district, string? date);
        //Task<(long, List<EventRegistration>?)> CheckListAttend(List<CheckAttendModel>? list);
        Task<int> ExportStatisticReport(ExportStatisticModel model);
    }
}
