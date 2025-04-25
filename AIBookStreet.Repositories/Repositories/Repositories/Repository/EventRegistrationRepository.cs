using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Data;
using AIBookStreet.Repositories.Repositories.Base;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AIBookStreet.Repositories.Repositories.Repositories.Repository
{
    public class EventRegistrationRepository(BSDbContext context) : BaseRepository<EventRegistration>(context), IEventRegistrationRepository
    {
        public async Task<List<EventRegistration>> GetAll(Guid eventId)
        {
            var queryable = GetQueryable();
            queryable = queryable.Where(z => !z.IsDeleted && z.EventId == eventId);
            var eventRegistrations = await queryable.ToListAsync();
            return eventRegistrations;
        }
        public async Task<EventRegistration?> GetByID(Guid? id)
        {
            var query = GetQueryable(z => z.Id == id);
            var eventRegistration = await query.Include(z => z.Event)
                                  .SingleOrDefaultAsync();

            return eventRegistration;
        }
        public async Task<EventRegistration?> GetByEmail(Guid? eventId, string email)
        {
            var query = GetQueryable(z => z.RegistrantEmail == email && z.EventId == eventId);
            var eventRegistration = await query.SingleOrDefaultAsync();

            return eventRegistration;
        }
        public async Task<(List<object>, List<object>, List<object>, List<object>, List<object>, int, int)> GetStatistic(Guid? eventId)
        {
            var queryable = GetQueryable();
            queryable = queryable.Where(z => !z.IsDeleted && z.EventId == eventId);
            var ageStatistic = new List<object>();
            var genderStatistic = new List<object>();
            var referenceStatistic = new List<object>();
            var addressStatistic = new List<object>();
            var hasAttendedBeforeStatic = new List<object>();

            var ageRangeCounts = await queryable.GroupBy(er => er.RegistrantAgeRange)
                                        .Select(group => new
                                        {
                                            AgeRange = group.Key,
                                            Count = group.Count()
                                        }).ToListAsync();
            var genderCount = await queryable.GroupBy(er => er.RegistrantGender)
                                        .Select(group => new
                                        {
                                            Gender = group.Key,
                                            Count = group.Count()
                                        }).ToListAsync();
            var referenceCount = await queryable.GroupBy(er => er.ReferenceSource)
                                        .Select(group => new
                                        {
                                            ReferenceSource = group.Key,
                                            Count = group.Count()
                                        }).ToListAsync();
            var addressCount = await queryable.GroupBy(er => er.RegistrantAddress)
                                        .Select(group => new
                                        {
                                            Address = group.Key,
                                            Count = group.Count()
                                        }).ToListAsync();
            var hasAttendedBeforeCount = await queryable.GroupBy(er => er.HasAttendedBefore)
                                        .Select(group => new
                                        {
                                            HasAttendedBefore = group.Key,
                                            Count = group.Count()
                                        }).ToListAsync();
            foreach ( var group in ageRangeCounts)
            {
                ageStatistic.Add(new
                {
                    Label = group.AgeRange.ToString(),
                    Value = group.Count
                });
            }
            foreach (var group in genderCount)
            {
                genderStatistic.Add(new
                {
                    Label = group.Gender.ToString(),
                    Value = group.Count
                });
            }
            foreach (var group in referenceCount)
            {
                referenceStatistic.Add(new
                {
                    Label = group.ReferenceSource.ToString(),
                    Value = group.Count
                });
            }
            foreach (var group in addressCount)
            {
                addressStatistic.Add(new
                {
                    Label = group.Address.ToString(),
                    Value = group.Count
                });
            }
            foreach (var group in hasAttendedBeforeCount)
            {
                hasAttendedBeforeStatic.Add(new
                {
                    Label = group.HasAttendedBefore ? "Đã tham dự các sự kiện tương tự" : "Chưa tham dự các sự kiện tương tự",
                    Value = group.Count
                });
            }
            var totalRegistrations = await queryable.CountAsync();
            var attend = await queryable.CountAsync(er => er.IsAttended);
            return (ageStatistic, genderStatistic, referenceStatistic, addressStatistic, hasAttendedBeforeStatic, totalRegistrations, attend);
        }
    }
}
