using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.UnitOfWork.Interface;
using AIBookStreet.Services.Base;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using FluentEmail.Core;
using Razor.Templating.Core;
using System.Net.Mail;
using System.Net.Mime;
using Microsoft.Extensions.Options;
using BarcodeStandard;
using static QRCoder.PayloadGenerator;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Globalization;

namespace AIBookStreet.Services.Services.Service
{
    public class EventRegistrationService(IUnitOfWork repository, IMapper mapper, IHttpContextAccessor httpContextAccessor, SmtpClient smtpClient, IOptions<SmtpSettings> smtpSettings, IRazorTemplateEngine razorTemplateEngine, ITicketService ticketService) : BaseService<EventRegistration>(mapper, repository, httpContextAccessor), IEventRegistrationService
    {
        private readonly IUnitOfWork _repository = repository;
        private readonly IRazorTemplateEngine _razorTemplateEngine = razorTemplateEngine;
        private readonly SmtpClient _smtpClient = smtpClient;
        private readonly SmtpSettings _smtpSettings = smtpSettings.Value;
        private readonly ITicketService _ticketService = ticketService;
        public async Task<(long, List<EventRegistration>?, string)> AddAnEventRegistration(EventRegistrationModel model)
        {
            try
            {
                var evt = await _repository.EventRepository.GetByID(model.EventId);
                if (evt == null)
                {
                    return (3, null, "Không tìm thấy sự kiện"); //khong tim thay
                }
                if (evt.EventSchedules?.OrderBy(es => es.EventDate).FirstOrDefault()?.EventDate <= DateOnly.FromDateTime(DateTime.Now))
                {
                    return (3, null, "Sự kiện đã/đang diễn ra, không thể đăng ký");
                }                

                if (!model.RegistrantGender.Equals("nam", StringComparison.CurrentCultureIgnoreCase) && !model.RegistrantGender.Equals("nữ", StringComparison.CurrentCultureIgnoreCase))
                {
                    return (3, null, "Vui lòng chọn giới tính là Nam hoặc Nữ.");
                }
                if (model.RegistrantAddress.Split(",").Length <3)
                {
                    return (3, null, "Vui lòng chọn đủ các trường địa chỉ");
                }
                var resp = new List<EventRegistration>();

                var ticket = await _ticketService.AddATicket(model.EventId);
                if (ticket.Item1 == 1)
                {
                    return (3, null, ticket.Item3 ?? "");
                }

                foreach (var date in model.DateToAttends)
                {
                    var existed = await _repository.EventRegistrationRepository.GetByEmail(model.EventId, model.RegistrantEmail, date);
                    if (existed != null)
                    {
                        if (ticket.Item2 != null)
                        {
                            await _repository.TicketRepository.Remove(ticket.Item2);
                        }
                        return (3, null, "Đã tồn tại người đăng ký email này trong ngày " + DateOnly.Parse(date).ToString("dd/MM") + " cho sự kiện này"); //da ton tai
                    }
                    var eventRegistrationModel = _mapper.Map<EventRegistration>(model);
                    eventRegistrationModel.IsAttended = false;
                    eventRegistrationModel.DateToAttend = DateOnly.Parse(date);
                    eventRegistrationModel.TicketId = ticket.Item2?.Id;
                    var setEventRegistrationModel = await SetBaseEntityToCreateFunc(eventRegistrationModel);
                    var isSuccess = await _repository.EventRegistrationRepository.Add(setEventRegistrationModel);
                    if (!isSuccess)
                    {
                        if (resp.Count > 0)
                        {
                            foreach (var e in resp)
                            {
                                await _repository.EventRegistrationRepository.Remove(e);
                            }
                        }
                        if (ticket.Item2 != null)
                        {
                            await _repository.TicketRepository.Remove(ticket.Item2);
                        }
                        return (3, null, "Đăng ký thất bại cho ngày " + DateOnly.Parse(date).ToString("dd/MM") + "!");
                    }
                    resp.Add(eventRegistrationModel);
                }

                return (2, resp, "Đăng ký thành công");
                
            } catch
            {
                throw;
            }
        }
        public async Task<(long, List<EventRegistration>?, string)> CheckAttend(List<CheckAttendModel> models, Event? evt)
        {
            try
            {
                var user = await GetUserInfo();
                var isOrganizer = false;
                if (user != null)
                {
                    foreach (var userRole in user.UserRoles)
                    {
                        if (userRole.Role.RoleName == "Organizer")
                        {
                            isOrganizer = true;
                            break;
                        }
                    }
                }
                if (!isOrganizer)
                {
                    return (0, null, "Hãy đăng nhập với vai trò Người tổ chức sự kiện");
                }
                if (models == null)
                {
                    return (0, null, "Danh sách điểm danh trống");
                }
                if (evt == null)
                {
                    return (0, null, "Sự kiện không hợp lệ");
                }
                if (evt.EventSchedules?.OrderBy(es => es.EventDate).FirstOrDefault()?.EventDate > DateOnly.FromDateTime(DateTime.Now.Date))
                {
                    return (0, null, "Chỉ có thể điểm danh trong ngày diễn ra sự kiện");
                }
                if (evt.EventSchedules?.OrderByDescending(es => es.EventDate).FirstOrDefault()?.EventDate < DateOnly.FromDateTime(DateTime.Now))
                {
                    return (0, null, "Chỉ có thể điểm danh trong ngày diễn ra sự kiện");
                }

                var resp = new List<EventRegistration>();
                foreach (var model in models)
                {
                    var existed = await _repository.EventRegistrationRepository.GetByIDForCheckIn(model.Id);
                    if (existed == null)
                    {
                        return (0, null, "Đơn đăng ký không tồn tại!!!"); //khong ton tai
                    }

                    if (existed.IsDeleted)
                    {
                        return (0, null, "Đơn đăng ký đã bị xóa");
                    }
                    if (!string.IsNullOrEmpty(model.TicketCode))
                    {
                        var validRegistrationInDate = await _repository.EventRegistrationRepository.GetRegistrationValidInDate(existed.TicketId);
                        if (model.TicketCode == validRegistrationInDate?.Ticket?.TicketCode)
                        {
                            if (validRegistrationInDate.IsAttended)
                            {
                                return (0 , null, "Vé đã được sử dụng");
                            }
                            else
                            {
                                validRegistrationInDate.IsAttended = true;
                                var name = user?.FullName?.Split(" ");
                                var updateBy = name?.Length > 1 ? name?[0][..1].ToUpper() : name?[0][..1].ToUpper() + name?[0][1..];
                                for (int i = 1; i < name?.Length; i++)
                                {
                                    updateBy += " " + name[i];
                                }
                                validRegistrationInDate.LastUpdatedBy = updateBy;
                                validRegistrationInDate.LastUpdatedDate = DateTime.Now;
                                var success = await _repository.EventRegistrationRepository.Update(validRegistrationInDate);
                                if (!success)
                                {
                                    return (0, null, "Đã xảy ra lỗi, không thể điểm danh cho '"+validRegistrationInDate.RegistrantName+"'!");       //update fail
                                }
                                resp.Add(validRegistrationInDate);
                            }
                        }
                        else
                        {
                            return (0, null, "Vé không hợp lệ");
                        }
                    }
                    else
                    {
                        existed.IsAttended = model.IsAttended;
                        var name = user?.FullName?.Split(" ");
                        var updateBy = name?.Length > 1 ? name?[0][..1].ToUpper() : name?[0][..1].ToUpper() + name?[0][1..];
                        for (int i = 1; i < name?.Length; i++)
                        {
                            updateBy += " " + name[i];
                        }
                        existed.LastUpdatedBy = updateBy;
                        existed.LastUpdatedDate = DateTime.Now;
                        var success = await _repository.EventRegistrationRepository.Update(existed);
                        if (!success)
                        {
                            return (0, null, "Đã xảy ra lỗi, không thể điểm danh cho '"+existed.RegistrantName+"'!");       //update fail
                        }
                        resp.Add(existed);
                    }
                }
                return (1, resp, "Điểm danh thành công"); //update thanh cong
            } catch
            {
                throw;
            }
        }
        //public async Task<(long, EventRegistration?)> DeleteAnEventRegistration(Guid id)
        //{
        //    var existed = await _repository.EventRegistrationRepository.GetByID(id);
        //    if (existed == null)
        //    {
        //        return (1, null); //khong ton tai
        //    }
        //    existed = await SetBaseEntityToUpdateFunc(existed);

        //    return await _repository.EventRegistrationRepository.Delete(existed) ? (2, existed) //delete thanh cong
        //                                                                  : (3, null);       //delete fail
        //}
        public async Task<EventRegistration?> GetAnEventRegistrationById(Guid? id)
        {
            try
            {
                return await _repository.EventRegistrationRepository.GetByID(id);
            } catch
            {
                throw;
            }
        }
        public async Task<(long, List<EventRegistration>?)>  GetAllActiveEventRegistrationsInAnEvent(Guid eventId, string? searchKey, string? date)
        {
            try
            {
                var user = await GetUserInfo();
                var isOrganizer = false;
                if (user != null)
                {
                    foreach (var userRole in user.UserRoles)
                    {
                        if (userRole.Role.RoleName == "Organizer")
                        {
                            isOrganizer = true;
                            break;
                        }
                    }
                }
                if (!isOrganizer)
                {
                    return (0, null);
                }
                var eventRegistrations = await _repository.EventRegistrationRepository.GetAll(eventId, searchKey, date);

                return eventRegistrations.Count == 0 ? (1, null) : (2, eventRegistrations);
            } catch
            {
                throw;
            }
        }
        public async Task<(List<object>, List<object>, List<object>, List<object>, List<object>, int, int)> GetAnEventStatistics(Guid eventId, bool? isAttended, string? province, string? district, string? date)
        {
            return await _repository.EventRegistrationRepository.GetStatistic(eventId, isAttended, province, district, date);
        }
        public async Task<int> SendRegistrationEmai(Ticket? ticket)
        {
            try
            {
                //var qrData = new
                //{
                //    id = ticket?.Id,
                //    ticketCode = ticket?.TicketCode,
                //    eventId = ticket?.EventRegistration?.Event?.EventName,
                //    registrationId = ticket?.RegistrationId,
                //    attendeeName = ticket?.EventRegistration?.RegistrantName,
                //    attendeeEmail = ticket?.EventRegistration?.RegistrantEmail,
                //    attendeePhone = ticket?.EventRegistration?.RegistrantPhoneNumber,
                //    attendeeAddress = ticket?.EventRegistration?.RegistrantAddress,
                //    eventName = ticket?.EventRegistration?.Event?.EventName,
                //    eventStartDate = ticket?.EventRegistration?.Event?.StartDate,
                //    eventEndDate = ticket?.EventRegistration?.Event?.EndDate,
                //    eventLocation = ticket?.EventRegistration?.Event?.Zone?.Street?.Address,
                //    zoneId = ticket?.EventRegistration?.Event?.ZoneId,
                //    zoneName = ticket?.EventRegistration?.Event?.Zone?.ZoneName,
                //    issuedAt = ticket?.CreatedDate
                //};
                //var qrData = ticket?.RegistrationId + " | " + ticket?.TicketCode + " | " + ticket?.EventRegistration?.RegistrantName;
                //    new
                //{
                //    id = ticket?.Id,
                //    ticketCode = ticket?.TicketCode,
                //    secretPassCode = ticket?.SecretPasscode,
                //    registrationId = ticket?.RegistrationId,
                //    issuedAt = ticket?.CreatedDate
                //};
                //string jsonData = JsonSerializer.Serialize(qrData);

                var qrData = ticket?.EventRegistrations?.FirstOrDefault()?.Id + " | " + ticket?.TicketCode + " | " + ticket?.EventRegistrations?.FirstOrDefault()?.RegistrantName;
                QRCodeGenerator qrGenerator = new();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(5);

                var barCodeInfor = ticket?.EventRegistrations?.FirstOrDefault()?.Id.ToString();
                Barcode barcode = new();
                barcode.Encode(BarcodeStandard.Type.Code128, barCodeInfor, 1200, 400);
                //string tempBarFilePath = Path.Combine(Path.GetTempPath(), "test-bar.png");
                //barcode.SaveImage(tempBarFilePath, SaveTypes.Png);
                using MemoryStream ms1 = new();
                barcode.SaveImage(ms1, SaveTypes.Png);
                ms1.Position = 0;

                var from = new MailAddress(_smtpSettings.From);
                var to = new MailAddress(ticket?.EventRegistrations?.FirstOrDefault()?.RegistrantEmail);
                var htmlBody = await _razorTemplateEngine.RenderAsync("ResponseModel/EventRegistration.cshtml", ticket);

                var mail = new MailMessage(from, to)
                {
                    Subject = "[SmartBookStreet] Thư cảm ơn",
                    IsBodyHtml = true
                };
                //string tempQRFilePath = Path.Combine(Path.GetTempPath(), "qrCode.png");
                //qrCodeImage.Save(tempQRFilePath, ImageFormat.Png);
                using MemoryStream ms2 = new();
                qrCodeImage.Save(ms2, ImageFormat.Png);
                ms2.Position = 0;
                var view = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                var image = new LinkedResource(ms2, MediaTypeNames.Image.Webp)
                {
                    ContentId = "qrImage",
                    TransferEncoding = TransferEncoding.Base64
                };
                var image2 = new LinkedResource(ms1, MediaTypeNames.Image.Webp)
                {
                    ContentId = "barImage",
                    TransferEncoding = TransferEncoding.Base64
                };
                view.LinkedResources.Add(image);
                view.LinkedResources.Add(image2);
                mail.AlternateViews.Add(view);

                await _smtpClient.SendMailAsync(mail);
                return 1;
            } catch
            {
                throw;
            }
        }
        //public async Task<(long, List<EventRegistration>?)> CheckListAttend(List<CheckAttendModel>? list)
        //{
        //    var user = await GetUserInfo();
        //    var isStaff = false;
        //    if (user != null)
        //    {
        //        foreach (var userRole in user.UserRoles)
        //        {
        //            if (userRole.Role.RoleName == "Staff")
        //            {
        //                isStaff = true;
        //            }
        //        }
        //    }
        //    if (!isStaff)
        //    {
        //        return (0, null);
        //    }
        //    if (list == null)
        //    {
        //        return (5, null);
        //    }
        //    var resp = new List<EventRegistration>();
        //    foreach (var model in list)
        //    {
        //        var existed = await _repository.EventRegistrationRepository.GetByID(model.Id);
        //        if (existed == null)
        //        {
        //            return (1, null); //khong ton tai
        //        }
        //        if (existed.Event.EndDate.Value <= DateTime.Now)
        //        {
        //            return (4, null);
        //        }
        //        if (existed.IsDeleted)
        //        {
        //            return (3, null);
        //        }

        //        existed.IsAttended = model.IsAttended;
        //        existed = await SetBaseEntityToUpdateFunc(existed);
        //        var success = await _repository.EventRegistrationRepository.Update(existed);
        //        if (!success)
        //        {
        //            return (3, null);       //update fail
        //        }
        //        resp.Add(existed);
        //    }           

        //    return (2, resp);
        //}

        public async Task<int> ExportStatisticReport(ExportStatisticModel model)
        {
            try
            {
                var dataList = await _repository.EventRegistrationRepository.GetAll(model.EventId, null, null);
                var eventData = await _repository.EventRepository.GetByID(model.EventId);
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

                    //======================Tỷ lệ tham gia============================================================================
                    ExcelWorksheet worksheet2 = excelPackage.Workbook.Worksheets.Add("Số liệu");

                    worksheet2.Cells[5, 13].Value = "Tham dự";
                    worksheet2.Cells[5, 13, 5, 14].Merge = true;
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
                    attendanceChart.Series.Add(worksheet2.Cells[7, 14, 8, 14], worksheet2.Cells[7, 13, 8, 13]);
                    attendanceChart.DataLabel.ShowPercent = true;

                    //==========================Kinh nghiệm=======================================================================
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

                    //=========================Giới tính=========================================================

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
                    //=========================Kênh truyền thông========================================================

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
                    referenceChart.SetSize(600, 400);
                    referenceChart.Title.Text = "Biểu đồ Truyền thông";

                    ExcelChartSerie seriesReference1 = referenceChart.Series.Add(worksheet2.Cells[60, 14, 65, 14], worksheet2.Cells[60, 13, 65, 13]);
                    seriesReference1.Header = "Đăng ký";
                    ExcelChartSerie seriesReference2 = referenceChart.Series.Add(worksheet2.Cells[60, 15, 65, 15], worksheet2.Cells[60, 13, 65, 13]);
                    seriesReference2.Header = "Tham gia";

                    //referenceChart.XAxis.Title.Text = "Nguồn";
                    referenceChart.XAxis.TextBody.Rotation = -45;
                    referenceChart.YAxis.Title.Text = "Số người";

                    //==========================Độ tuổi=======================================================
                    worksheet2.Cells[80, 14].Value = "Đăng ký";
                    worksheet2.Cells[80, 15].Value = "Tham gia";
                    worksheet2.Cells[81, 13].Value = "Dưới 12 tuổi";
                    worksheet2.Cells[82, 13].Value = "13-17 tuổi";
                    worksheet2.Cells[83, 13].Value = "18-24 tuổi";
                    worksheet2.Cells[84, 13].Value = "25-34 tuổi";
                    worksheet2.Cells[85, 13].Value = "35-44 tuổi";
                    worksheet2.Cells[86, 13].Value = "45-54 tuổi";
                    worksheet2.Cells[87, 13].Value = "Trên 55 tuổi";

                    using (ExcelRange headerRange = worksheet2.Cells[80, 14, 80, 15])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    using (ExcelRange headerRange = worksheet2.Cells[81, 13, 87, 13])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    worksheet2.Cells[81, 14].Value = dataList.Where(er => er.RegistrantAgeRange.Equals("Dưới 12 tuổi")).Count();
                    worksheet2.Cells[82, 14].Value = dataList.Where(er => er.RegistrantAgeRange.Equals("13-17 tuổi")).Count();
                    worksheet2.Cells[83, 14].Value = dataList.Where(er => er.RegistrantAgeRange.Equals("18-24 tuổi")).Count();
                    worksheet2.Cells[84, 14].Value = dataList.Where(er => er.RegistrantAgeRange.Equals("25-34 tuổi")).Count();
                    worksheet2.Cells[85, 14].Value = dataList.Where(er => er.RegistrantAgeRange.Equals("35-44 tuổi")).Count();
                    worksheet2.Cells[86, 14].Value = dataList.Where(er => er.RegistrantAgeRange.Equals("45-54 tuổi")).Count();
                    worksheet2.Cells[87, 14].Value = dataList.Where(er => er.RegistrantAgeRange.Equals("Trên 55 tuổi")).Count();
                    worksheet2.Cells[81, 15].Value = dataList.Where(er => er.IsAttended == true && er.RegistrantAgeRange.Equals("Dưới 12 tuổi")).Count();
                    worksheet2.Cells[82, 15].Value = dataList.Where(er => er.IsAttended == true && er.RegistrantAgeRange.Equals("13-17 tuổi")).Count();
                    worksheet2.Cells[83, 15].Value = dataList.Where(er => er.IsAttended == true && er.RegistrantAgeRange.Equals("18-24 tuổi")).Count();
                    worksheet2.Cells[84, 15].Value = dataList.Where(er => er.IsAttended == true && er.RegistrantAgeRange.Equals("25-34 tuổi")).Count();
                    worksheet2.Cells[85, 15].Value = dataList.Where(er => er.IsAttended == true && er.RegistrantAgeRange.Equals("35-44 tuổi")).Count();
                    worksheet2.Cells[86, 15].Value = dataList.Where(er => er.IsAttended == true && er.RegistrantAgeRange.Equals("45-54 tuổi")).Count();
                    worksheet2.Cells[87, 15].Value = dataList.Where(er => er.IsAttended == true && er.RegistrantAgeRange.Equals("Trên 55 tuổi")).Count();

                    using (ExcelRange tableRange = worksheet2.Cells[81, 13, 87, 15])
                    {
                        tableRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        tableRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    };
                    using (ExcelRange tableRange = worksheet2.Cells[80, 14, 80, 15])
                    {
                        tableRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        tableRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    };

                    ExcelLineChart? ageChart = worksheet2.Drawings.AddChart("AgeLineChart", eChartType.Line) as ExcelLineChart;
                    ageChart.SetPosition(77, 0, 1, 0);
                    ageChart.SetSize(600, 400);
                    ageChart.Title.Text = "Biểu đồ Nhóm tuổi";

                    ExcelChartSerie seriesAge1 = ageChart.Series.Add(worksheet2.Cells[81, 14, 87, 14], worksheet2.Cells[81, 13, 87, 13]);
                    seriesAge1.Header = "Đăng ký";
                    ExcelChartSerie seriesAge2 = ageChart.Series.Add(worksheet2.Cells[81, 15, 87, 15], worksheet2.Cells[81, 13, 87, 13]);
                    seriesAge2.Header = "Tham gia";

                    //ageChart.XAxis.Title.Text = "Độ tuổi";
                    referenceChart.XAxis.TextBody.Rotation = -45;
                    ageChart.YAxis.Title.Text = "Số người";

                    //===========================Địa điểm======================================================
                    worksheet2.Cells[102, 16].Value = "Đăng ký";
                    worksheet2.Cells[102, 17].Value = "Tham gia";

                    using (ExcelRange headerRange = worksheet2.Cells[102, 16, 102, 17])
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
                    var addressIndex = 103;
                    foreach (var address in addressCount)
                    {
                        worksheet2.Cells[addressIndex, 15].Value = address.Address.Trim().ToString();
                        using (ExcelRange headerRange = worksheet2.Cells[addressIndex, 15])
                        {
                            headerRange.Style.Font.Bold = true;
                            headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        }
                        worksheet2.Cells[addressIndex, 16].Value = address.Count;
                        worksheet2.Cells[addressIndex, 17].Value = address.CountAttended;
                        addressIndex++;
                    }

                    using (ExcelRange tableRange = worksheet2.Cells[102, 16, 102, 17])
                    {
                        tableRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        tableRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    };
                    using (ExcelRange tableRange = worksheet2.Cells[103, 15, addressIndex - 1, 17])
                    {
                        tableRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        tableRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    };

                    ExcelLineChart? addressChart = worksheet2.Drawings.AddChart("AddressLineChart", eChartType.Line) as ExcelLineChart;
                    addressChart.SetPosition(100, 0, 1, 0);
                    addressChart.SetSize(800, 400);
                    addressChart.Title.Text = "Biểu đồ địa điểm";

                    ExcelChartSerie seriesAddress1 = addressChart.Series.Add(worksheet2.Cells[103, 16, addressIndex - 1, 16], worksheet2.Cells[103, 15, addressIndex - 1, 15]);
                    seriesAddress1.Header = "Đăng ký";
                    ExcelChartSerie seriesAddress2 = addressChart.Series.Add(worksheet2.Cells[103, 17, addressIndex - 1, 17], worksheet2.Cells[103, 15, addressIndex - 1, 15]);
                    seriesAddress2.Header = "Tham gia";

                    //ageChart.XAxis.Title.Text = "Địa điểm";
                    referenceChart.XAxis.TextBody.Rotation = -45;
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
                    var to = new MailAddress(model.Email != null ? model.Email : eventData.OrganizerEmail);
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
                return 1;
            }
            catch
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
