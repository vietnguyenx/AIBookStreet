﻿using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
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
        Task<int> SendEmai(Ticket? registration);
        Task<(long, List<EventRegistration>?, string)> CheckAttend(List<CheckAttendModel> models, Event? evt);
        //Task<(long, EventRegistration?)> DeleteAnEventRegistration(Guid id);
        Task<EventRegistration?> GetAnEventRegistrationById(Guid? id);
        Task<(long, List<EventRegistration>?)> GetAllActiveEventRegistrations(Guid eventId, string? searchKey);
        Task<(List<object>, List<object>, List<object>, List<object>, List<object>, int, int)> Test (Guid eventId, bool? isAttend);
        //Task<(long, List<EventRegistration>?)> CheckListAttend(List<CheckAttendModel>? list);
    }
}
