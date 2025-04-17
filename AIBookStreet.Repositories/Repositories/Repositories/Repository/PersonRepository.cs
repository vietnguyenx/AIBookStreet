using AIBookStreet.Repositories.Data;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Repository
{
    public class PersonRepository : BaseRepository<Person>, IPersonRepository
    {
        private readonly BSDbContext _context;

        public PersonRepository(BSDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> SyncPersonsFromAPI(List<Person> persons)
        {
            if (persons == null || !persons.Any())
                return false;

            // Find existing persons by external id
            var externalIds = persons.Select(p => p.ExternalId).ToList();
            var existingPersons = await _context.Persons
                .Where(p => externalIds.Contains(p.ExternalId))
                .ToDictionaryAsync(p => p.ExternalId, p => p);

            var personsToAdd = new List<Person>();
            var personsToUpdate = new List<Person>();

            foreach (var person in persons)
            {
                if (existingPersons.TryGetValue(person.ExternalId, out var existingPerson))
                {
                    // Update existing person
                    existingPerson.Gender = person.Gender;
                    existingPerson.Features = person.Features;
                    existingPerson.FirstSeen = person.FirstSeen;
                    existingPerson.LastSeen = person.LastSeen;
                    existingPerson.DailyAppearances = person.DailyAppearances;
                    existingPerson.TotalAppearances = person.TotalAppearances;
                    existingPerson.ExternalCreatedAt = person.ExternalCreatedAt;
                    existingPerson.ExternalUpdatedAt = person.ExternalUpdatedAt;
                    existingPerson.LastUpdatedDate = DateTime.Now;

                    personsToUpdate.Add(existingPerson);
                }
                else
                {
                    // Add new person
                    person.CreatedDate = DateTime.Now;
                    personsToAdd.Add(person);
                }
            }

            if (personsToAdd.Any())
                await AddRange(personsToAdd);

            if (personsToUpdate.Any())
                await UpdateRange(personsToUpdate);

            return true;
        }

        public async Task<int> GetTotalPersonCount()
        {
            return await _context.Persons.CountAsync(p => !p.IsDeleted);
        }

        public async Task<(int currentMonthCount, int previousMonthCount)> GetCurrentAndPreviousMonthCount()
        {
            var today = DateTime.Today;
            
            // Current month
            var currentMonthStart = new DateTime(today.Year, today.Month, 1);
            var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);
            
            // Previous month
            var previousMonthStart = currentMonthStart.AddMonths(-1);
            var previousMonthEnd = currentMonthStart.AddDays(-1);
            
            var currentMonthCount = await _context.Persons
                .CountAsync(p => !p.IsDeleted && 
                            p.FirstSeen >= currentMonthStart && 
                            p.FirstSeen <= currentMonthEnd);
                            
            var previousMonthCount = await _context.Persons
                .CountAsync(p => !p.IsDeleted && 
                            p.FirstSeen >= previousMonthStart && 
                            p.FirstSeen <= previousMonthEnd);
                            
            return (currentMonthCount, previousMonthCount);
        }

        public async Task<int> GetPersonCountByGender(string gender)
        {
            return await _context.Persons
                .CountAsync(p => !p.IsDeleted && p.Gender.ToLower() == gender.ToLower());
        }

        public async Task<Dictionary<string, int>> GetPersonCountByDay(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1).AddSeconds(-1);

            var personsQuery = _context.Persons
                .Where(p => !p.IsDeleted && 
                            p.FirstSeen >= startDate && 
                            p.FirstSeen <= endDate);

            var maleCount = await personsQuery
                .CountAsync(p => p.Gender.ToLower() == "male");

            var femaleCount = await personsQuery
                .CountAsync(p => p.Gender.ToLower() == "female");

            var totalCount = await personsQuery.CountAsync();

            return new Dictionary<string, int>
            {
                { "male", maleCount },
                { "female", femaleCount },
                { "total", totalCount }
            };
        }

        public async Task<Dictionary<string, int>> GetPersonCountByMonth(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddSeconds(-1);

            var personsQuery = _context.Persons
                .Where(p => !p.IsDeleted && 
                            p.FirstSeen >= startDate && 
                            p.FirstSeen <= endDate);

            var maleCount = await personsQuery
                .CountAsync(p => p.Gender.ToLower() == "male");

            var femaleCount = await personsQuery
                .CountAsync(p => p.Gender.ToLower() == "female");

            var totalCount = await personsQuery.CountAsync();

            return new Dictionary<string, int>
            {
                { "male", maleCount },
                { "female", femaleCount },
                { "total", totalCount }
            };
        }

        public async Task<Dictionary<string, int>> GetPersonCountByYear(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = startDate.AddYears(1).AddSeconds(-1);

            var personsQuery = _context.Persons
                .Where(p => !p.IsDeleted && 
                            p.FirstSeen >= startDate && 
                            p.FirstSeen <= endDate);

            var maleCount = await personsQuery
                .CountAsync(p => p.Gender.ToLower() == "male");

            var femaleCount = await personsQuery
                .CountAsync(p => p.Gender.ToLower() == "female");

            var totalCount = await personsQuery.CountAsync();

            return new Dictionary<string, int>
            {
                { "male", maleCount },
                { "female", femaleCount },
                { "total", totalCount }
            };
        }

        public async Task<Dictionary<DateTime, int>> GetDailyAppearancesByDateRange(DateTime startDate, DateTime endDate)
        {
            var result = new Dictionary<DateTime, int>();
            
            var appearances = await _context.Persons
                .Where(p => !p.IsDeleted && 
                            p.FirstSeen.Date >= startDate.Date && 
                            p.FirstSeen.Date <= endDate.Date)
                .GroupBy(p => p.FirstSeen.Date)
                .Select(g => new 
                {
                    Date = g.Key,
                    TotalAppearances = g.Sum(p => p.DailyAppearances)
                })
                .ToListAsync();

            foreach (var appearance in appearances)
            {
                result.Add(appearance.Date, appearance.TotalAppearances);
            }

            return result;
        }

        public async Task<Dictionary<DateTime, Dictionary<string, int>>> GetDailyAppearancesByDateRangeAndGender(DateTime startDate, DateTime endDate)
        {
            var result = new Dictionary<DateTime, Dictionary<string, int>>();
            
            var maleAppearances = await _context.Persons
                .Where(p => !p.IsDeleted && 
                            p.Gender.ToLower() == "male" &&
                            p.FirstSeen.Date >= startDate.Date && 
                            p.FirstSeen.Date <= endDate.Date)
                .GroupBy(p => p.FirstSeen.Date)
                .Select(g => new 
                {
                    Date = g.Key,
                    Gender = "male",
                    TotalAppearances = g.Sum(p => p.DailyAppearances)
                })
                .ToListAsync();

            var femaleAppearances = await _context.Persons
                .Where(p => !p.IsDeleted && 
                            p.Gender.ToLower() == "female" &&
                            p.FirstSeen.Date >= startDate.Date && 
                            p.FirstSeen.Date <= endDate.Date)
                .GroupBy(p => p.FirstSeen.Date)
                .Select(g => new 
                {
                    Date = g.Key,
                    Gender = "female",
                    TotalAppearances = g.Sum(p => p.DailyAppearances)
                })
                .ToListAsync();

            var allDates = maleAppearances.Select(a => a.Date)
                .Union(femaleAppearances.Select(a => a.Date))
                .OrderBy(d => d)
                .ToList();

            foreach (var date in allDates)
            {
                var maleCount = maleAppearances
                    .FirstOrDefault(a => a.Date == date)?.TotalAppearances ?? 0;
                
                var femaleCount = femaleAppearances
                    .FirstOrDefault(a => a.Date == date)?.TotalAppearances ?? 0;

                result.Add(date, new Dictionary<string, int>
                {
                    { "male", maleCount },
                    { "female", femaleCount },
                    { "total", maleCount + femaleCount }
                });
            }

            return result;
        }

        public async Task<List<object>> GetVisitorStatsByDateRange(DateTime startDate, DateTime endDate)
        {
            var result = new List<object>();
            
            // Get all dates in the range
            var currentDate = startDate.Date;
            var datesList = new List<DateTime>();
            
            while (currentDate <= endDate.Date)
            {
                datesList.Add(currentDate);
                currentDate = currentDate.AddDays(1);
            }
            
            // Query data for each day in the date range
            foreach (var date in datesList)
            {
                var dayStart = date;
                var dayEnd = date.AddDays(1).AddSeconds(-1);
                
                var personsQuery = _context.Persons
                    .Where(p => !p.IsDeleted && 
                                p.FirstSeen >= dayStart && 
                                p.FirstSeen <= dayEnd);
                
                var maleCount = await personsQuery
                    .CountAsync(p => p.Gender.ToLower() == "male");
                
                var femaleCount = await personsQuery
                    .CountAsync(p => p.Gender.ToLower() == "female");
                
                var totalCount = await personsQuery.CountAsync();
                
                // Create the data object in the required format
                result.Add(new
                {
                    day = date.ToString("yyyy-MM-dd"),
                    visitor = totalCount,
                    male = maleCount,
                    female = femaleCount
                });
            }
            
            return result;
        }
    }
} 