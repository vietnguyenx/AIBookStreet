using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IPersonService
    {
        Task<bool> SyncPersonsFromAPI();
        Task<int> GetTotalPersonCount();
        Task<int> GetPersonCountByGender(string gender);
        Task<Dictionary<string, int>> GetPersonCountByDay(DateTime date);
        Task<Dictionary<string, int>> GetPersonCountByMonth(int year, int month);
        Task<Dictionary<string, int>> GetPersonCountByYear(int year);
        Task<Dictionary<DateTime, int>> GetDailyAppearancesByDateRange(DateTime startDate, DateTime endDate);
        Task<Dictionary<DateTime, Dictionary<string, int>>> GetDailyAppearancesByDateRangeAndGender(DateTime startDate, DateTime endDate);
        Task<List<object>> GetVisitorStatsByDateRange(DateTime startDate, DateTime endDate);
    }
} 