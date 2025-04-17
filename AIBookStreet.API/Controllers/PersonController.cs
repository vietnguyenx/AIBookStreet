using AIBookStreet.API.ResponseModel;
using AIBookStreet.Services.Common;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Linq;
using System.Collections.Generic;

namespace AIBookStreet.API.Controllers
{
    [Route("api/persons")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly IPersonService _personService;

        public PersonController(IPersonService personService)
        {
            _personService = personService;
        }

        /// <summary>
        /// Đồng bộ dữ liệu người từ API bên ngoài
        /// </summary>
        [HttpPost("sync")]
        public async Task<IActionResult> SyncPersonsFromAPI()
        {
            try
            {
                var result = await _personService.SyncPersonsFromAPI();
                return result 
                    ? Ok(new BaseResponse(true, "Đồng bộ dữ liệu thành công"))
                    : BadRequest(new BaseResponse(false, "Đồng bộ dữ liệu thất bại"));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, 
                    new BaseResponse(false, $"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy tổng số người
        /// </summary>
        [HttpGet("stats/total")]
        public async Task<IActionResult> GetTotalPersonCount()
        {
            try
            {
                var result = await _personService.GetTotalPersonCountWithChangePercent();
                return Ok(new
                {
                    success = true,
                    total = result.totalCount,
                    currentMonthPercentChange = result.percentChange,
                    changeDirection = result.percentChange > 0 ? "increase" : (result.percentChange < 0 ? "decrease" : "unchanged")
                });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, 
                    new BaseResponse(false, $"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy số lượng người theo giới tính
        /// </summary>
        [HttpGet("stats/gender/{gender}")]
        public async Task<IActionResult> GetPersonCountByGender(string gender)
        {
            try
            {
                var result = await _personService.GetPersonCountByGender(gender);
                return Ok(new
                {
                    success = true,
                    gender = gender,
                    count = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new BaseResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, 
                    new BaseResponse(false, $"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy thống kê số lượng người theo ngày cụ thể
        /// </summary>
        [HttpGet("stats/daily")]
        public async Task<IActionResult> GetPersonCountByDay([FromQuery] DateTime date)
        {
            try
            {
                var result = await _personService.GetPersonCountByDay(date);
                
                // Chuyển đổi dữ liệu sang dạng array cho biểu đồ
                var chartData = new List<object>
                {
                    new { label = "Nam", value = result.ContainsKey("male") ? result["male"] : 0 },
                    new { label = "Nữ", value = result.ContainsKey("female") ? result["female"] : 0 }
                };
                
                return Ok(new
                {
                    success = true,
                    date = date.ToString("yyyy-MM-dd"),
                    statistics = result, // Dữ liệu gốc
                    chartData = chartData, // Dữ liệu cho biểu đồ
                    total = result.ContainsKey("total") ? result["total"] : 0
                });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, 
                    new BaseResponse(false, $"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy thống kê số lượng người theo tháng
        /// </summary>
        [HttpGet("stats/monthly/{year}/{month}")]
        public async Task<IActionResult> GetPersonCountByMonth(int year, int month)
        {
            try
            {
                var result = await _personService.GetPersonCountByMonth(year, month);
                
                // Chuyển đổi dữ liệu sang dạng array cho biểu đồ
                var chartData = new List<object>
                {
                    new { label = "Nam", value = result.ContainsKey("male") ? result["male"] : 0 },
                    new { label = "Nữ", value = result.ContainsKey("female") ? result["female"] : 0 }
                };
                
                return Ok(new
                {
                    success = true,
                    year = year,
                    month = month,
                    statistics = result, // Dữ liệu gốc
                    chartData = chartData, // Dữ liệu cho biểu đồ
                    total = result.ContainsKey("total") ? result["total"] : 0
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new BaseResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, 
                    new BaseResponse(false, $"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy thống kê số lượng người theo năm
        /// </summary>
        [HttpGet("stats/yearly/{year}")]
        public async Task<IActionResult> GetPersonCountByYear(int year)
        {
            try
            {
                var result = await _personService.GetPersonCountByYear(year);
                
                // Chuyển đổi dữ liệu sang dạng array cho biểu đồ
                var chartData = new List<object>
                {
                    new { label = "Nam", value = result.ContainsKey("male") ? result["male"] : 0 },
                    new { label = "Nữ", value = result.ContainsKey("female") ? result["female"] : 0 }
                };
                
                return Ok(new
                {
                    success = true,
                    year = year,
                    statistics = result, // Dữ liệu gốc
                    chartData = chartData, // Dữ liệu cho biểu đồ
                    total = result.ContainsKey("total") ? result["total"] : 0
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new BaseResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, 
                    new BaseResponse(false, $"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy thống kê số lần xuất hiện theo khoảng thời gian
        /// </summary>
        [HttpGet("stats/range")]
        public async Task<IActionResult> GetDailyAppearancesByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var result = await _personService.GetVisitorStatsByDateRange(startDate, endDate);
                return Ok(new
                {
                    success = true,
                    barData = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new BaseResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, 
                    new BaseResponse(false, $"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy thống kê số lần xuất hiện theo khoảng thời gian và giới tính
        /// </summary>
        [HttpGet("stats/range-gender")]
        public async Task<IActionResult> GetDailyAppearancesByDateRangeAndGender([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var result = await _personService.GetDailyAppearancesByDateRangeAndGender(startDate, endDate);
                
                // Chuyển đổi dữ liệu sang dạng array cho biểu đồ
                var orderedDates = result.Keys.OrderBy(d => d).ToList();
                var labels = orderedDates.Select(d => d.ToString("yyyy-MM-dd")).ToArray();
                
                var maleValues = orderedDates.Select(date => 
                    result[date].ContainsKey("male") ? result[date]["male"] : 0).ToArray();
                    
                var femaleValues = orderedDates.Select(date => 
                    result[date].ContainsKey("female") ? result[date]["female"] : 0).ToArray();
                    
                var totalValues = orderedDates.Select(date => 
                    result[date].ContainsKey("total") ? result[date]["total"] : 0).ToArray();
                
                var chartData = new
                {
                    labels = labels,
                    datasets = new[] 
                    {
                        new 
                        {
                            label = "Nam",
                            data = maleValues
                        },
                        new 
                        {
                            label = "Nữ",
                            data = femaleValues
                        },
                        new 
                        {
                            label = "Tổng",
                            data = totalValues
                        }
                    }
                };
                
                return Ok(new
                {
                    success = true,
                    startDate = startDate.ToString("yyyy-MM-dd"),
                    endDate = endDate.ToString("yyyy-MM-dd"),
                    statistics = result.ToDictionary(x => x.Key.ToString("yyyy-MM-dd"), x => x.Value), // Dữ liệu gốc
                    chartData = chartData // Dữ liệu cho biểu đồ
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new BaseResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, 
                    new BaseResponse(false, $"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy thời gian trung bình một người có mặt tại đường sách
        /// </summary>
        [HttpGet("stats/average-time")]
        public async Task<IActionResult> GetAveragePresenceTime()
        {
            try
            {
                var result = await _personService.GetAveragePresenceTime();
                
                // Format the TimeSpan objects to make them more readable
                var formattedAverageTime = FormatTimeSpan(result.averageTime);
                var formattedAverageTimeByGender = result.averageTimeByGender.ToDictionary(
                    kvp => kvp.Key,
                    kvp => FormatTimeSpan(kvp.Value)
                );
                
                // Prepare chart data
                var chartData = new List<object>
                {
                    new { 
                        label = "Nam", 
                        value = (int)result.averageTimeByGender["male"].TotalMinutes,
                        time = FormatTimeHHmm(result.averageTimeByGender["male"])
                    },
                    new { 
                        label = "Nữ", 
                        value = (int)result.averageTimeByGender["female"].TotalMinutes,
                        time = FormatTimeHHmm(result.averageTimeByGender["female"])
                    }
                };
                
                return Ok(new
                {
                    success = true,
                    averageTime = formattedAverageTime,
                    averageTimeByGender = formattedAverageTimeByGender,
                    chartData = chartData,
                    averageTimeMinutes = (int)result.averageTime.TotalMinutes
                });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, 
                    new BaseResponse(false, $"Lỗi: {ex.Message}"));
            }
        }
        
        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
            {
                return $"{(int)timeSpan.TotalHours} giờ {timeSpan.Minutes} phút";
            }
            else
            {
                return $"{timeSpan.Minutes} phút {timeSpan.Seconds} giây";
            }
        }

        private string FormatTimeHHmm(TimeSpan timeSpan)
        {
            int hours = (int)timeSpan.TotalHours;
            int minutes = timeSpan.Minutes;
            return $"{hours:D2}{minutes:D2}";
        }
        
        /// <summary>
        /// Lấy thống kê số lượng khách theo giờ
        /// </summary>
        [HttpGet("stats/hourly")]
        public async Task<IActionResult> GetVisitorCountsByHour([FromQuery] DateTime? date = null)
        {
            try
            {
                var result = await _personService.GetVisitorCountsByHour(date);
                
                // Chuyển đổi kết quả sang định dạng phù hợp cho biểu đồ
                var hours = Enumerable.Range(0, 24).Select(h => $"{h:D2}:00").ToArray();
                var maleValues = Enumerable.Range(0, 24).Select(h => result[h]["male"]).ToArray();
                var femaleValues = Enumerable.Range(0, 24).Select(h => result[h]["female"]).ToArray();
                var totalValues = Enumerable.Range(0, 24).Select(h => result[h]["total"]).ToArray();
                
                var chartData = new
                {
                    labels = hours,
                    datasets = new[] 
                    {
                        new 
                        {
                            label = "Nam",
                            data = maleValues
                        },
                        new 
                        {
                            label = "Nữ",
                            data = femaleValues
                        },
                        new 
                        {
                            label = "Tổng",
                            data = totalValues
                        }
                    }
                };
                
                // Thống kê thêm về giờ cao điểm
                var peakHour = Enumerable.Range(0, 24)
                    .OrderByDescending(h => result[h]["total"])
                    .First();
                
                var quietHour = Enumerable.Range(0, 24)
                    .Where(h => result[h]["total"] > 0)  // Chỉ xét những giờ có khách
                    .OrderBy(h => result[h]["total"])
                    .FirstOrDefault();
                    
                // Tính trung bình khách mỗi giờ
                var totalVisitors = Enumerable.Range(0, 24)
                    .Sum(h => result[h]["total"]);
                    
                var hourWithVisitors = Enumerable.Range(0, 24)
                    .Count(h => result[h]["total"] > 0);
                    
                var averageVisitorsPerActiveHour = hourWithVisitors > 0
                    ? Math.Round((double)totalVisitors / hourWithVisitors, 2)
                    : 0;
                
                return Ok(new
                {
                    success = true,
                    date = date?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd"),
                    hourlyStats = result,
                    chartData = chartData,
                    peakHour = new
                    {
                        hour = $"{peakHour:D2}:00",
                        visitors = result[peakHour]["total"],
                        male = result[peakHour]["male"],
                        female = result[peakHour]["female"]
                    },
                    quietestHour = new
                    {
                        hour = $"{quietHour:D2}:00",
                        visitors = result[quietHour]["total"],
                        male = result[quietHour]["male"],
                        female = result[quietHour]["female"]
                    },
                    averageStats = new
                    {
                        averageVisitorsPerActiveHour = averageVisitorsPerActiveHour,
                        totalVisitors = totalVisitors,
                        activeHours = hourWithVisitors
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, 
                    new BaseResponse(false, $"Lỗi: {ex.Message}"));
            }
        }
    }
} 