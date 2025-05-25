using AIBookStreet.Services.Services.Interface;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;
using System.Net.Mime;
using FluentEmail.Core;
using Razor.Templating.Core;
using AIBookStreet.Repositories.Data.Entities;
using QRCoder;
using System.Drawing.Imaging;
using System.Drawing;
using System.Text.Json;
using FluentEmail.Core.Models;
using Microsoft.Extensions.Logging;
using SQLitePCL;
using AIBookStreet.Services.Model;
using Microsoft.Extensions.Options;
using BarcodeStandard;
using Microsoft.SqlServer.Server;
using static System.Runtime.InteropServices.JavaScript.JSType;
using OfficeOpenXml;
using AIBookStreet.Repositories.Repositories.UnitOfWork.Interface;
using Attachment = System.Net.Mail.Attachment;
using System.Globalization;
using System.Text;
using OfficeOpenXml.Drawing.Chart;
using static OfficeOpenXml.ExcelErrorValue;
using OfficeOpenXml.Style;

namespace AIBookStreet.Services.Services.Service
{
    public class QRGeneratorService(IUnitOfWork repository, SmtpClient smtpClient, IOptions<SmtpSettings> smtpSettings, IRazorTemplateEngine razorTemplateEngine) : IQRGeneratorService
    {
        public readonly IUnitOfWork _repository = repository;
        private readonly IRazorTemplateEngine _razorTemplateEngine = razorTemplateEngine;
        private readonly SmtpClient _smtpClient = smtpClient;
        private readonly SmtpSettings _smtpSettings = smtpSettings.Value;
        public async Task<Task> ExportListToExcel(string email)
        {
            try
            {
                var dataList = await _repository.EventRegistrationRepository.GetAll(Guid.Parse("C6856363-4AA4-4DE0-B271-5CD6B84E3E6F"), null, null);
                var eventData = await _repository.EventRepository.GetByID(Guid.Parse("C6856363-4AA4-4DE0-B271-5CD6B84E3E6F"));
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (ExcelPackage excelPackage = new())
                {
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Thông tin đăng ký");

                    // Ghi header
                    worksheet.Cells[1, 1].Value = "STT";
                    worksheet.Cells[1, 2].Value = "Họ và tên";
                    worksheet.Cells[1, 3].Value = "Độ tuổi";
                    worksheet.Cells[1, 4].Value = "Giới tính";
                    worksheet.Cells[1, 5].Value = "Địa chỉ";
                    worksheet.Cells[1, 6].Value = "Email";
                    worksheet.Cells[1, 7].Value = "SĐT";
                    worksheet.Cells[1, 8].Value = "Nguồn biết đến";
                    worksheet.Cells[1, 9].Value = "Tham dự";

                    // Định dạng header (tùy chọn)
                    using (ExcelRange headerRange = worksheet.Cells[1, 1, 1, 9])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Aqua);
                    }
                    var i = 0;
                    // Ghi dữ liệu từ danh sách
                    foreach (var data in dataList)
                    {
                        worksheet.Cells[i + 2, 1].Value = i;
                        worksheet.Cells[i + 2, 2].Value = data.RegistrantName;
                        worksheet.Cells[i + 2, 3].Value = data.RegistrantAgeRange;
                        worksheet.Cells[i + 2, 4].Value = data.RegistrantGender;
                        worksheet.Cells[i + 2, 5].Value = data.RegistrantAddress;
                        worksheet.Cells[i + 2, 6].Value = data.RegistrantEmail;
                        worksheet.Cells[i + 2, 7].Value = data.RegistrantPhoneNumber;
                        worksheet.Cells[i + 2, 8].Value = data.ReferenceSource;
                        worksheet.Cells[i + 2, 9].Value = data.IsAttended ? "X" : "";
                        i++;
                    }

                    using (ExcelRange tableRange = worksheet.Cells[1, 1, i + 1, 9])
                    {
                        tableRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    }
                    // Tự động điều chỉnh độ rộng cột
                    worksheet.Cells.AutoFitColumns();

                    //==================================================================================================

                    ExcelWorksheet worksheet2 = excelPackage.Workbook.Worksheets.Add("Số liệu");

                    worksheet2.Cells[5, 13].Value = "Tham dự";
                    worksheet2.Cells[5,13,5,14].Merge = true;
                    using (ExcelRange headerRange = worksheet2.Cells[5, 13, 5, 14])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }                    

                    worksheet2.Cells[6, 13].Value = "Tổng lượt đăng ký";
                    worksheet2.Cells[7, 13].Value = "Tổng lượt tham gia";
                    worksheet2.Cells[8, 13].Value = "Tổng lượt vắng"; 
                    worksheet2.Cells[9, 13].Value = "Tỷ lệ tham gia";

                    using (ExcelRange headerRange = worksheet2.Cells[6, 13, 9, 13])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }
                    var rate = (decimal)dataList.Where(er => er.IsAttended == true).Count() * 100 / dataList.Count;
                    worksheet2.Cells[6, 14].Value = dataList.Count;
                    worksheet2.Cells[7, 14].Value = dataList.Where(er => er.IsAttended == true).Count();
                    worksheet2.Cells[8, 14].Value = dataList.Where(er => er.IsAttended == false).Count();
                    worksheet2.Cells[9, 14].Value = rate.ToString("0.00") + "%";

                    using (ExcelRange tableRange = worksheet2.Cells[5, 13, 9, 14])
                    { 
                        tableRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        tableRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    };
                        

                    ExcelPieChart? attendanceChart = worksheet2.Drawings.AddChart("AttendancePieChart", eChartType.Pie) as ExcelPieChart;
                    attendanceChart.SetPosition(1, 0, 1, 0);
                    attendanceChart.SetSize(600, 300);
                    attendanceChart.Title.Text = "Biểu đồ tỷ lệ tham gia";
                    attendanceChart.Series.Add(worksheet2.Cells[7, 14, 8 ,14], worksheet2.Cells[7, 13, 8, 13]);
                    attendanceChart.DataLabel.ShowPercent = true;

                    //=================================================================================================
                    worksheet2.Cells[24, 14].Value = "Có kinh nghiệm";
                    worksheet2.Cells[24, 15].Value = "Chưa có kinh nghiệm";
                    worksheet2.Cells[25, 13].Value = "Đăng ký";
                    worksheet2.Cells[26, 13].Value = "Tham gia";

                    using (ExcelRange headerRange = worksheet2.Cells[24, 14, 24, 15])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    using (ExcelRange headerRange = worksheet2.Cells[25, 13, 26, 13])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    worksheet2.Cells[25, 14].Value = dataList.Where(er => er.HasAttendedBefore == true).Count();
                    worksheet2.Cells[25, 15].Value = dataList.Where(er => er.HasAttendedBefore == false).Count(); 
                    worksheet2.Cells[26, 14].Value = dataList.Where(er => er.IsAttended == true && er.HasAttendedBefore == true).Count(); 
                    worksheet2.Cells[26, 15].Value = dataList.Where(er => er.IsAttended == true && er.HasAttendedBefore == false).Count();

                    using (ExcelRange tableRange = worksheet2.Cells[25, 13, 26, 15])
                    {
                        tableRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        tableRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    };
                    using (ExcelRange tableRange = worksheet2.Cells[24, 14, 24, 15])
                    {
                        tableRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        tableRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    };

                    ExcelChart sampleExpChart = worksheet2.Drawings.AddChart("ExperientColumnClusteredChart", eChartType.ColumnClustered);
                    sampleExpChart.SetPosition(19, 0, 1, 0);
                    sampleExpChart.SetSize(600, 300);
                    sampleExpChart.Title.Text = "Biểu đồ kinh nghiệm";

                    ExcelChartSerie seriesExperient1 = sampleExpChart.Series.Add(worksheet2.Cells[25, 14, 25, 15], worksheet2.Cells[24, 14, 24, 15]);
                    seriesExperient1.Header = "Đăng ký";
                    ExcelChartSerie seriesExperient2 = sampleExpChart.Series.Add(worksheet2.Cells[26, 14, 26, 15], worksheet2.Cells[24, 14, 24, 15]);
                    seriesExperient2.Header = "Tham gia";

                    //sampleExpChart.XAxis.Title.Text = "Kinh nghiệm";
                    sampleExpChart.YAxis.Title.Text = "Số người";

                    //==================================================================================

                    worksheet2.Cells[42, 14].Value = "Giới tính";
                    worksheet2.Cells[42, 14, 42, 15].Merge = true;
                    worksheet2.Cells[43, 14].Value = "Nam";
                    worksheet2.Cells[43, 15].Value = "Nữ";
                    worksheet2.Cells[44, 13].Value = "Đăng ký";
                    worksheet2.Cells[45, 13].Value = "Tham gia";

                    using (ExcelRange headerRange = worksheet2.Cells[42, 14, 43, 15])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    using (ExcelRange headerRange = worksheet2.Cells[44, 13, 45, 13])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    worksheet2.Cells[44, 14].Value = dataList.Where(er => er.RegistrantGender.ToLower().Equals("nam")).Count();
                    worksheet2.Cells[44, 15].Value = dataList.Where(er => er.RegistrantGender.ToLower().Equals("nữ")).Count();
                    worksheet2.Cells[45, 14].Value = dataList.Where(er => er.IsAttended == true && er.RegistrantGender.ToLower().Equals("nam")).Count();
                    worksheet2.Cells[45, 15].Value = dataList.Where(er => er.IsAttended == true && er.RegistrantGender.ToLower().Equals("nữ")).Count();

                    using (ExcelRange tableRange = worksheet2.Cells[42, 14, 45, 15])
                    {
                        tableRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        tableRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    };
                    using (ExcelRange tableRange = worksheet2.Cells[44, 13, 45, 13])
                    {
                        tableRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        tableRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    };

                    ExcelChart genderChart = worksheet2.Drawings.AddChart("GenderColumnClusteredChart", eChartType.ColumnClustered);
                    genderChart.SetPosition(38, 0, 1, 0);
                    genderChart.SetSize(600, 300);
                    genderChart.Title.Text = "Biểu đồ giới tính";

                    ExcelChartSerie seriesGender1 = genderChart.Series.Add(worksheet2.Cells[44, 14, 44, 15], worksheet2.Cells[43, 14, 43, 15]);
                    seriesGender1.Header = "Đăng ký";
                    ExcelChartSerie seriesGender2 = genderChart.Series.Add(worksheet2.Cells[45, 14, 45, 15], worksheet2.Cells[43, 14, 43, 15]);
                    seriesGender2.Header = "Tham gia";

                    //genderChart.XAxis.Title.Text = "Giới tính";
                    genderChart.YAxis.Title.Text = "Số người";
                    //=================================================================================

                    worksheet2.Cells[59, 14].Value = "Đăng ký";
                    worksheet2.Cells[59, 15].Value = "Tham gia";
                    worksheet2.Cells[60, 13].Value = "Mạng xã hội";
                    worksheet2.Cells[61, 13].Value = "Bạn bè giới thiệu";
                    worksheet2.Cells[62, 13].Value = "Trang web Đường sách";
                    worksheet2.Cells[63, 13].Value = "Email thông báo";
                    worksheet2.Cells[64, 13].Value = "Tình cờ biết được";
                    worksheet2.Cells[65, 13].Value = "Khác";

                    using (ExcelRange headerRange = worksheet2.Cells[59, 14, 59, 15])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    using (ExcelRange headerRange = worksheet2.Cells[60, 13, 65, 13])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    worksheet2.Cells[60, 14].Value = dataList.Where(er => er.ReferenceSource.Equals("Mạng xã hội")).Count();
                    worksheet2.Cells[61, 14].Value = dataList.Where(er => er.ReferenceSource.Equals("Bạn bè giới thiệu")).Count();
                    worksheet2.Cells[62, 14].Value = dataList.Where(er => er.ReferenceSource.Equals("Trang web Đường sách")).Count();
                    worksheet2.Cells[63, 14].Value = dataList.Where(er => er.ReferenceSource.Equals("Email thông báo")).Count();
                    worksheet2.Cells[64, 14].Value = dataList.Where(er => er.ReferenceSource.Equals("Tình cờ biết được")).Count();
                    worksheet2.Cells[65, 14].Value = dataList.Where(er => er.ReferenceSource.Equals("Khác")).Count();
                    worksheet2.Cells[60, 15].Value = dataList.Where(er => er.IsAttended == true && er.ReferenceSource.Equals("Mạng xã hội")).Count();
                    worksheet2.Cells[61, 15].Value = dataList.Where(er => er.IsAttended == true && er.ReferenceSource.Equals("Bạn bè giới thiệu")).Count();
                    worksheet2.Cells[62, 15].Value = dataList.Where(er => er.IsAttended == true && er.ReferenceSource.Equals("Trang web đường sách")).Count();
                    worksheet2.Cells[63, 15].Value = dataList.Where(er => er.IsAttended == true && er.ReferenceSource.Equals("Email thông báo")).Count();
                    worksheet2.Cells[64, 15].Value = dataList.Where(er => er.IsAttended == true && er.ReferenceSource.Equals("Tình cờ biết được")).Count();
                    worksheet2.Cells[65, 15].Value = dataList.Where(er => er.IsAttended == true && er.ReferenceSource.Equals("Khác")).Count();

                    using (ExcelRange tableRange = worksheet2.Cells[60, 13, 65, 15])
                    {
                        tableRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        tableRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    };
                    using (ExcelRange tableRange = worksheet2.Cells[59, 14, 59, 15])
                    {
                        tableRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        tableRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    };

                    ExcelLineChart? referenceChart = worksheet2.Drawings.AddChart("ReferenceLineChart", eChartType.Line) as ExcelLineChart;
                    referenceChart.SetPosition(55, 0, 1, 0);
                    referenceChart.SetSize(600, 300);
                    referenceChart.Title.Text = "Biểu đồ nguồn tiếp cận";
                                        
                    ExcelChartSerie seriesReference1 = referenceChart.Series.Add(worksheet2.Cells[60, 14, 65, 14], worksheet2.Cells[60, 13, 65, 13]);
                    seriesReference1.Header = "Đăng ký";
                    ExcelChartSerie seriesReference2 = referenceChart.Series.Add(worksheet2.Cells[60, 15, 65, 15], worksheet2.Cells[60, 13, 65, 13]);
                    seriesReference2.Header = "Tham gia";

                    //referenceChart.XAxis.Title.Text = "Nguồn";
                    referenceChart.YAxis.Title.Text = "Số người";

                    //=================================================================================
                    worksheet2.Cells[75, 14].Value = "Đăng ký";
                    worksheet2.Cells[75, 15].Value = "Tham gia";
                    worksheet2.Cells[76, 13].Value = "Dưới 12 tuổi";
                    worksheet2.Cells[77, 13].Value = "13-17 tuổi";
                    worksheet2.Cells[78, 13].Value = "18-24 tuổi";
                    worksheet2.Cells[79, 13].Value = "25-34 tuổi";
                    worksheet2.Cells[80, 13].Value = "35-44 tuổi";
                    worksheet2.Cells[81, 13].Value = "45-54 tuổi";
                    worksheet2.Cells[82, 13].Value = "Trên 55 tuổi";

                    using (ExcelRange headerRange = worksheet2.Cells[75, 14, 75, 15])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    using (ExcelRange headerRange = worksheet2.Cells[76, 13, 82, 13])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    worksheet2.Cells[76, 14].Value = dataList.Where(er => er.RegistrantAgeRange.Equals("Dưới 12 tuổi")).Count();
                    worksheet2.Cells[77, 14].Value = dataList.Where(er => er.RegistrantAgeRange.Equals("13-17 tuổi")).Count();
                    worksheet2.Cells[78, 14].Value = dataList.Where(er => er.RegistrantAgeRange.Equals("18-24 tuổi")).Count();
                    worksheet2.Cells[79, 14].Value = dataList.Where(er => er.RegistrantAgeRange.Equals("25-34 tuổi")).Count();
                    worksheet2.Cells[80, 14].Value = dataList.Where(er => er.RegistrantAgeRange.Equals("35-44 tuổi")).Count();
                    worksheet2.Cells[81, 14].Value = dataList.Where(er => er.RegistrantAgeRange.Equals("45-54 tuổi")).Count();
                    worksheet2.Cells[82, 14].Value = dataList.Where(er => er.RegistrantAgeRange.Equals("Trên 55 tuổi")).Count();
                    worksheet2.Cells[76, 15].Value = dataList.Where(er => er.IsAttended == true && er.RegistrantAgeRange.Equals("Dưới 12 tuổi")).Count();
                    worksheet2.Cells[77, 15].Value = dataList.Where(er => er.IsAttended == true && er.RegistrantAgeRange.Equals("13-17 tuổi")).Count();
                    worksheet2.Cells[78, 15].Value = dataList.Where(er => er.IsAttended == true && er.RegistrantAgeRange.Equals("18-24 tuổi")).Count();
                    worksheet2.Cells[79, 15].Value = dataList.Where(er => er.IsAttended == true && er.RegistrantAgeRange.Equals("25-34 tuổi")).Count();
                    worksheet2.Cells[80, 15].Value = dataList.Where(er => er.IsAttended == true && er.RegistrantAgeRange.Equals("35-44 tuổi")).Count();
                    worksheet2.Cells[81, 15].Value = dataList.Where(er => er.IsAttended == true && er.RegistrantAgeRange.Equals("45-54 tuổi")).Count();
                    worksheet2.Cells[82, 15].Value = dataList.Where(er => er.IsAttended == true && er.RegistrantAgeRange.Equals("Trên 55 tuổi")).Count();

                    using (ExcelRange tableRange = worksheet2.Cells[76, 13, 82, 15])
                    {
                        tableRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        tableRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    };
                    using (ExcelRange tableRange = worksheet2.Cells[75, 14, 75, 15])
                    {
                        tableRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        tableRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    };

                    ExcelLineChart? ageChart = worksheet2.Drawings.AddChart("AgeLineChart", eChartType.Line) as ExcelLineChart;
                    ageChart.SetPosition(72, 0, 1, 0);
                    ageChart.SetSize(600, 300);
                    ageChart.Title.Text = "Biểu đồ nguồn tiếp cận";

                    ExcelChartSerie seriesAge1 = ageChart.Series.Add(worksheet2.Cells[76, 14, 82, 14], worksheet2.Cells[76, 13, 82, 13]);
                    seriesAge1.Header = "Đăng ký";
                    ExcelChartSerie seriesAge2 = ageChart.Series.Add(worksheet2.Cells[76, 15, 82, 15], worksheet2.Cells[76, 13, 82, 13]);
                    seriesAge2.Header = "Tham gia";

                    //ageChart.XAxis.Title.Text = "Độ tuổi";
                    ageChart.YAxis.Title.Text = "Số người";

                    //=================================================================================
                    worksheet2.Cells[92, 14].Value = "Đăng ký";
                    worksheet2.Cells[92, 15].Value = "Tham gia";

                    using (ExcelRange headerRange = worksheet2.Cells[92, 14, 92, 15])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    var addressCount = dataList.GroupBy(er => er.RegistrantAddress.Split(",")[2])
                                               .Select(group => new
                                                {
                                                    Address = group.Key,
                                                    Count = group.Count(),
                                                    CountAttended = group.Where(er => er.IsAttended == true).Count()
                                                });
                    var addressIndex = 93;
                    foreach (var address in addressCount)
                    {
                        worksheet2.Cells[addressIndex, 13].Value = address.Address.Trim().ToString();
                        using (ExcelRange headerRange = worksheet2.Cells[addressIndex, 13])
                        {
                            headerRange.Style.Font.Bold = true;
                            headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        }
                        worksheet2.Cells[addressIndex, 14].Value = address.Count;
                        worksheet2.Cells[addressIndex, 15].Value = address.CountAttended;
                        addressIndex++;
                    }

                    using (ExcelRange tableRange = worksheet2.Cells[92, 14, 92, 15])
                    {
                        tableRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        tableRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    };
                    using (ExcelRange tableRange = worksheet2.Cells[93, 13, addressIndex - 1, 15])
                    {
                        tableRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        tableRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    };

                    ExcelLineChart? addressChart = worksheet2.Drawings.AddChart("AddressLineChart", eChartType.Line) as ExcelLineChart;
                    addressChart.SetPosition(90, 0, 1, 0);
                    addressChart.SetSize(600, 300);
                    addressChart.Title.Text = "Biểu đồ địa điểm";

                    ExcelChartSerie seriesAddress1 = addressChart.Series.Add(worksheet2.Cells[93, 14, addressIndex - 1, 14], worksheet2.Cells[93, 13, addressIndex - 1, 13]);
                    seriesAddress1.Header = "Đăng ký";
                    ExcelChartSerie seriesAddress2 = addressChart.Series.Add(worksheet2.Cells[93, 15, addressIndex - 1, 15], worksheet2.Cells[93, 13, addressIndex - 1, 13]);
                    seriesAddress2.Header = "Tham gia";

                    //ageChart.XAxis.Title.Text = "Địa điểm";
                    addressChart.YAxis.Title.Text = "Số người";


                    // Lưu file Excel
                    //FileInfo excelFile = new("D:\\output_epplus_list.xlsx");
                    //excelPackage.SaveAs(excelFile);

                    worksheet2.Cells.AutoFitColumns();

                    //==============================================================================================
                    using MemoryStream ms = new();
                    excelPackage.SaveAs(ms);
                    ms.Position = 0;

                    var from = new MailAddress(_smtpSettings.From);
                    var to = new MailAddress(email);
                    var htmlBody = await _razorTemplateEngine.RenderAsync("ResponseModel/Index.cshtml", eventData);

                    var mail = new MailMessage(from, to)
                    {
                        Subject = "[SmartBookStreet] Lời cảm ơn sau sự kiện",
                        IsBodyHtml = true
                    };
                    var view = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                    mail.AlternateViews.Add(view);
                    var fileName = DateTime.Now.ToString("yyyyMMdd") + "_BaoCaoSuKien_" + ToUnaccentedPascalCase(eventData?.EventName) + ".xlsx";
                    Attachment attachment = new(ms, fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                    mail.Attachments.Add(attachment);

                    await _smtpClient.SendMailAsync(mail);
                }

                Console.WriteLine($"File Excel đã được tạo thành công");
                return Task.CompletedTask;
            } catch
            {
                throw;
            }
        }
        public static string? ToUnaccentedPascalCase(string? input)
        {
            try
            {
                if (input != null)
                {
                    // Bước 1: Loại bỏ dấu tiếng Việt
                    string normalized = input.Normalize(NormalizationForm.FormD);
                    var sb = new StringBuilder();

                    foreach (char c in normalized)
                    {
                        var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                        if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                        {
                            sb.Append(c);
                        }
                    }

                    string noDiacritics = sb.ToString().Normalize(NormalizationForm.FormC);

                    // Bước 2: Viết hoa chữ cái đầu từng từ và loại bỏ khoảng trắng
                    TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
                    string[] words = noDiacritics.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string result = string.Concat(words.Select(w => textInfo.ToTitleCase(w.ToLower())));

                    return result;
                }
                return null;
            }
            catch
            {
                throw;
            }
        }
    }
}
