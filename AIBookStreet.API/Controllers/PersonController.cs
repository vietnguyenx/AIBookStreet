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
                var result = await _personService.GetTotalPersonCount();
                return Ok(new
                {
                    success = true,
                    total = result
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
                var result = await _personService.GetDailyAppearancesByDateRange(startDate, endDate);
                
                // Chuyển đổi dữ liệu sang dạng array cho biểu đồ
                var labels = result.Keys.OrderBy(d => d).Select(d => d.ToString("yyyy-MM-dd")).ToArray();
                var values = result.OrderBy(kv => kv.Key).Select(kv => kv.Value).ToArray();
                
                var chartData = new
                {
                    labels = labels,
                    datasets = new[] 
                    {
                        new 
                        {
                            label = "Tổng lượt xuất hiện",
                            data = values
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
    }
} 