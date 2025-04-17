using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using AIBookStreet.Repositories.Repositories.UnitOfWork.Interface;
using AIBookStreet.Repositories.Repositories.Utils;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Service
{
    public class PersonService : IPersonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PersonService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<bool> SyncPersonsFromAPI()
        {
            try
            {
                // Lấy dữ liệu từ API
                var personsFromApi = await PersonApiHelper.FetchPersonsFromApi();
                if (personsFromApi == null || !personsFromApi.Any())
                    return false;

                // Đồng bộ dữ liệu vào database
                return await _unitOfWork.PersonRepository.SyncPersonsFromAPI(personsFromApi);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                Console.WriteLine($"Lỗi khi đồng bộ dữ liệu từ API: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetTotalPersonCount()
        {
            return await _unitOfWork.PersonRepository.GetTotalPersonCount();
        }

        public async Task<(int totalCount, double percentChange)> GetTotalPersonCountWithChangePercent()
        {
            var totalCount = await _unitOfWork.PersonRepository.GetTotalPersonCount();
            var monthCounts = await _unitOfWork.PersonRepository.GetCurrentAndPreviousMonthCount();
            
            double percentChange = 0;
            
            if (monthCounts.previousMonthCount > 0)
            {
                percentChange = ((double)monthCounts.currentMonthCount - monthCounts.previousMonthCount) / monthCounts.previousMonthCount * 100;
            }
            
            return (totalCount, Math.Round(percentChange, 2));
        }

        public async Task<int> GetPersonCountByGender(string gender)
        {
            if (string.IsNullOrEmpty(gender))
                throw new ArgumentException("Giới tính không được để trống");

            return await _unitOfWork.PersonRepository.GetPersonCountByGender(gender);
        }

        public async Task<Dictionary<string, int>> GetPersonCountByDay(DateTime date)
        {
            return await _unitOfWork.PersonRepository.GetPersonCountByDay(date);
        }

        public async Task<Dictionary<string, int>> GetPersonCountByMonth(int year, int month)
        {
            if (year <= 0)
                throw new ArgumentException("Năm không hợp lệ");

            if (month <= 0 || month > 12)
                throw new ArgumentException("Tháng không hợp lệ");

            return await _unitOfWork.PersonRepository.GetPersonCountByMonth(year, month);
        }

        public async Task<Dictionary<string, int>> GetPersonCountByYear(int year)
        {
            if (year <= 0)
                throw new ArgumentException("Năm không hợp lệ");

            return await _unitOfWork.PersonRepository.GetPersonCountByYear(year);
        }

        public async Task<Dictionary<DateTime, int>> GetDailyAppearancesByDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Ngày bắt đầu phải trước ngày kết thúc");

            return await _unitOfWork.PersonRepository.GetDailyAppearancesByDateRange(startDate, endDate);
        }

        public async Task<Dictionary<DateTime, Dictionary<string, int>>> GetDailyAppearancesByDateRangeAndGender(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Ngày bắt đầu phải trước ngày kết thúc");

            return await _unitOfWork.PersonRepository.GetDailyAppearancesByDateRangeAndGender(startDate, endDate);
        }

        public async Task<List<object>> GetVisitorStatsByDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Ngày bắt đầu phải trước ngày kết thúc");

            return await _unitOfWork.PersonRepository.GetVisitorStatsByDateRange(startDate, endDate);
        }

        public async Task<(TimeSpan averageTime, Dictionary<string, TimeSpan> averageTimeByGender)> GetAveragePresenceTime()
        {
            return await _unitOfWork.PersonRepository.GetAveragePresenceTime();
        }

        public async Task<Dictionary<int, Dictionary<string, int>>> GetVisitorCountsByHour(DateTime? date = null)
        {
            return await _unitOfWork.PersonRepository.GetVisitorCountsByHour(date);
        }
    }
} 