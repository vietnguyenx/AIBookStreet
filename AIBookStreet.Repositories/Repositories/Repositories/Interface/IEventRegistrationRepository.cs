﻿using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface IEventRegistrationRepository : IBaseRepository<EventRegistration>
    {
        Task<List<EventRegistration>> GetAll(Guid eventId);
        Task<EventRegistration?> GetByID(Guid? id);
        Task<EventRegistration?> GetByEmail(Guid? eventId, string email);
        Task<(List<object>, List<object>, List<object>, List<object>, List<object>)> GetStatistic(Guid? eventId);
    }
}
