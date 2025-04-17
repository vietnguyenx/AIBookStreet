using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface IPersonRepository : IBaseRepository<Person>
    {
        Task<bool> SyncPersonsFromAPI(List<Person> persons);
        Task<int> GetTotalPersonCount();
        Task<(int currentMonthCount, int previousMonthCount)> GetCurrentAndPreviousMonthCount();
        Task<int> GetPersonCountByGender(string gender);
        Task<Dictionary<string, int>> GetPersonCountByDay(DateTime date);
        Task<Dictionary<string, int>> GetPersonCountByMonth(int year, int month);
        Task<Dictionary<string, int>> GetPersonCountByYear(int year);
        Task<Dictionary<DateTime, int>> GetDailyAppearancesByDateRange(DateTime startDate, DateTime endDate);
        Task<Dictionary<DateTime, Dictionary<string, int>>> GetDailyAppearancesByDateRangeAndGender(DateTime startDate, DateTime endDate);
        Task<List<object>> GetVisitorStatsByDateRange(DateTime startDate, DateTime endDate);
        Task<(TimeSpan averageTime, Dictionary<string, TimeSpan> averageTimeByGender)> GetAveragePresenceTime();
    }
} 