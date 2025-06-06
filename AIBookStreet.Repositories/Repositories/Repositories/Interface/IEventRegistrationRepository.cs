﻿using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface IEventRegistrationRepository : IBaseRepository<EventRegistration>
    {
        Task<List<EventRegistration>> GetAll(Guid eventId, string? searchKey, string? date);
        Task<EventRegistration?> GetByID(Guid? id);
        Task<EventRegistration?> GetByEmail(Guid? eventId, string email, string date);
        Task<(List<object>, List<object>, List<object>, List<object>, List<object>, int, int)> GetStatistic(Guid eventId, bool? isAttended, string? province, string? district, string? date);
        Task<EventRegistration?> GetByIDForCheckIn(Guid? id);
        Task<EventRegistration?> GetRegistrationValidInDate(Guid? ticketId);
    }
}
