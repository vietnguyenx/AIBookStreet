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

namespace AIBookStreet.Services.Services.Service
{
    public class EventRegistrationService(IUnitOfWork repository, IMapper mapper, IHttpContextAccessor httpContextAccessor, SmtpClient smtpClient, IOptions<SmtpSettings> smtpSettings, IRazorTemplateEngine razorTemplateEngine, ITicketService ticketService) : BaseService<EventRegistration>(mapper, repository, httpContextAccessor), IEventRegistrationService
    {
        private readonly IUnitOfWork _repository = repository;
        private readonly IRazorTemplateEngine _razorTemplateEngine = razorTemplateEngine;
        private readonly SmtpClient _smtpClient = smtpClient;
        private readonly SmtpSettings _smtpSettings = smtpSettings.Value;
        private readonly ITicketService _ticketService = ticketService;
        public async Task<(long, EventRegistration?, string?)> AddAnEventRegistration(EventRegistrationModel model)
        {
            try
            {
                var evt = await _repository.EventRepository.GetByID(model.EventId);
                if (evt == null)
                {
                    return (3, null, "Không tìm thấy sự kiện"); //khong tim thay
                }
                if (evt.EventSchedules.OrderBy(es => es.EventDate).FirstOrDefault()?.EventDate <= DateOnly.FromDateTime(DateTime.Now))
                {
                    return (3, null, "Sự kiện đã/đang diễn ra, không thể đăng ký");
                }
                var existed = await _repository.EventRegistrationRepository.GetByEmail(model.EventId, model.RegistrantEmail);
                if (existed != null)
                {
                    return (3, null, "Đã tồn tại người đăng ký email này cho sự kiện này"); //da ton tai
                }

                var eventRegistrationModel = _mapper.Map<EventRegistration>(model);
                eventRegistrationModel.IsAttended = false;
                var setEventRegistrationModel = await SetBaseEntityToCreateFunc(eventRegistrationModel);
                var isSuccess = await _repository.EventRegistrationRepository.Add(setEventRegistrationModel);
                if (isSuccess)
                {
                    return (2, setEventRegistrationModel, null);
                }
                return (3, null, "Đăng ký thất bại");
            } catch
            {
                throw;
            }
        }
        public async Task<(long, List<EventRegistration>?)> CheckAttend(List<CheckAttendModel> models, Event? evt)
        {
            try
            {
                var user = await GetUserInfo();
                var isStaff = false;
                if (user != null)
                {
                    foreach (var userRole in user.UserRoles)
                    {
                        if (userRole.Role.RoleName == "Staff")
                        {
                            isStaff = true;
                            break;
                        }
                    }
                }
                if (!isStaff)
                {
                    return (0, null);
                }
                if (models == null)
                {
                    return (6, null);
                }
                if (evt == null)
                {
                    return (7, null);
                }
                if (evt.EventSchedules?.OrderBy(es => es.EventDate).FirstOrDefault()?.EventDate > DateOnly.FromDateTime(DateTime.Now.Date))
                {
                    return (4, null);
                }
                if (evt.EventSchedules?.OrderByDescending(es => es.EventDate).FirstOrDefault()?.EventDate < DateOnly.FromDateTime(DateTime.Now))
                {
                    return (4, null);
                }

                var resp = new List<EventRegistration>();
                foreach (var model in models)
                {
                    var existed = await _repository.EventRegistrationRepository.GetByIDForCheckIn(model.Id);
                    if (existed == null)
                    {
                        return (1, null); //khong ton tai
                    }

                    if (existed.IsDeleted)
                    {
                        return (3, null);
                    }
                    if (!string.IsNullOrEmpty(model.TicketCode))
                    {
                        if (model.Id == existed.Id && model.TicketCode == existed.Ticket?.TicketCode)
                        {
                            if (existed.IsAttended)
                            {
                                return (5, null);
                            }
                            else
                            {
                                existed.IsAttended = true;
                                var name = user?.FullName?.Split(" ");
                                var updateBy = name?[0][..1].ToUpper();
                                for (int i = 1; i < name?.Length; i++)
                                {
                                    updateBy += " " + name[i];
                                }
                                existed.LastUpdatedBy = updateBy;
                                existed.LastUpdatedDate = DateTime.Now;
                                var success = await _repository.EventRegistrationRepository.Update(existed);
                                if (!success)
                                {
                                    return (3, null);       //update fail
                                }
                                resp.Add(existed);
                            }
                        }
                        else
                        {
                            return (5, null);
                        }
                    }
                    else
                    {
                        existed.IsAttended = model.IsAttended;
                        var name = user?.FullName?.Split(" ");
                        var updateBy = name?[0][..1].ToUpper();
                        for (int i = 1; i < name?.Length; i++)
                        {
                            updateBy += " " + name[i];
                        }
                        existed.LastUpdatedBy = updateBy;
                        existed.LastUpdatedDate = DateTime.Now;
                        var success = await _repository.EventRegistrationRepository.Update(existed);
                        if (!success)
                        {
                            return (3, null);       //update fail
                        }
                        resp.Add(existed);
                    }
                }
                return (2, resp); //update thanh cong
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
        public async Task<EventRegistration?> GetAnEventRegistrationById(Guid id)
        {
            try
            {
                return await _repository.EventRegistrationRepository.GetByID(id);
            } catch
            {
                throw;
            }
        }
        public async Task<(long, List<EventRegistration>?)>  GetAllActiveEventRegistrations(Guid eventId, string? searchKey)
        {
            try
            {
                var user = await GetUserInfo();
                var isStaff = false;
                if (user != null)
                {
                    foreach (var userRole in user.UserRoles)
                    {
                        if (userRole.Role.RoleName == "Staff")
                        {
                            isStaff = true;
                            break;
                        }
                    }
                }
                if (!isStaff)
                {
                    return (0, null);
                }
                var eventRegistrations = await _repository.EventRegistrationRepository.GetAll(eventId, searchKey);

                return eventRegistrations.Count == 0 ? (1, null) : (2, eventRegistrations);
            } catch
            {
                throw;
            }
        }
        public async Task<(List<object>, List<object>, List<object>, List<object>, List<object>, int, int)> Test (Guid eventId, bool? isAttend)
        {
            return await _repository.EventRegistrationRepository.GetStatistic(eventId, isAttend);
        }
        public async Task<int> SendEmai(Ticket? ticket)
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
                var qrData = ticket?.RegistrationId + " | " + ticket?.TicketCode + " | " + ticket?.EventRegistration?.RegistrantName;
                //    new
                //{
                //    id = ticket?.Id,
                //    ticketCode = ticket?.TicketCode,
                //    secretPassCode = ticket?.SecretPasscode,
                //    registrationId = ticket?.RegistrationId,
                //    issuedAt = ticket?.CreatedDate
                //};
                //string jsonData = JsonSerializer.Serialize(qrData);
                QRCodeGenerator qrGenerator = new();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(5);

                var barCodeInfor = ticket?.RegistrationId.ToString();
                Barcode barcode = new();
                barcode.Encode(BarcodeStandard.Type.Code128, barCodeInfor, 1200, 400);
                //string tempBarFilePath = Path.Combine(Path.GetTempPath(), "test-bar.png");
                //barcode.SaveImage(tempBarFilePath, SaveTypes.Png);
                using MemoryStream ms1 = new();
                barcode.SaveImage(ms1, SaveTypes.Png);
                ms1.Position = 0;

                var from = new MailAddress(_smtpSettings.From);
                var to = new MailAddress(ticket.EventRegistration.RegistrantEmail);
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
        public async Task<EventRegistration?> Remove(EventRegistration model)
        {
            try
            {
                var existed = await _repository.EventRegistrationRepository.GetByID(model.Id);
                if (existed == null)
                {
                    return null; //khong ton tai
                }

                return await _repository.EventRegistrationRepository.Remove(existed) ? existed //delete thanh cong
                                                                              : null;       //delete fail
            }
            catch
            {
                throw;
            }
        }
    }
}
