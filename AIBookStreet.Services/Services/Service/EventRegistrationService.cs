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
        public async Task<(long, Ticket?)> AddAnEventRegistration(EventRegistrationModel model)
        {
            var existed = await _repository.EventRegistrationRepository.GetByEmail(model.EventId, model.RegistrantEmail);
            if (existed != null)
            {
                return (1, null); //da ton tai
            }
            var eventRegistrationModel = _mapper.Map<EventRegistration>(model);
            var setEventRegistrationModel = await SetBaseEntityToCreateFunc(eventRegistrationModel);
            var isSuccess = await _repository.EventRegistrationRepository.Add(setEventRegistrationModel);
            if (isSuccess)
            {
                var evtRegis = await _repository.EventRegistrationRepository.GetByID(setEventRegistrationModel.Id);
               // await SendEmail(evtRegis);

                var ticketCode = GenerateRandomString(10);
                var ticketCodeExist = _repository.TicketRepository.SearchTicketCode(evtRegis.EventId, ticketCode);
                while (ticketCodeExist != null)
                {
                    ticketCode = GenerateRandomString(10);
                    ticketCodeExist = _repository.TicketRepository.SearchTicketCode(evtRegis.EventId, ticketCode);
                }
                var secretPasscode = GenerateRandomNumber(1000, 999999);
                var secretPasscodeExist = _repository.TicketRepository.SearchSecretPasscode(evtRegis.EventId, secretPasscode.ToString());
                while (secretPasscodeExist != null)
                {
                    secretPasscode = GenerateRandomNumber(1000, 999999);
                    secretPasscodeExist = _repository.TicketRepository.SearchSecretPasscode(evtRegis.EventId, secretPasscode.ToString());
                }

                var ticket = new TicketModel { 
                    TicketCode = ticketCode,
                    SecretPasscode = secretPasscode.ToString(),
                    RegistrationId = setEventRegistrationModel.Id,
                };
                var addTicket = await _ticketService.AddATicket(ticket);
                if (addTicket != null)
                {
                    var setTicket = await _repository.TicketRepository.GetByID(addTicket.Id);

                    return (2, setTicket);
                }

                return (3, null);
            }
            return (3, null);
        }
        //public async Task<(long, EventRegistration?)> UpdateAnEventRegistration(Guid? id, EventRegistrationModel model)
        //{
        //    var existed = await _repository.EventRegistrationRepository.GetByID(id);
        //    if (existed == null)
        //    {
        //        return (1, null); //khong ton tai
        //    }
        //    if (existed.IsDeleted)
        //    {
        //        return (3, null);
        //    }
        //    existed.RegistrantName = model.RegistrantName;
        //    existed.RegistrantEmail = model.RegistrantEmail;
        //    existed.RegistrantPhoneNumber = model.RegistrantPhoneNumber;
        //    existed.RegistrantAgeRange = model.RegistrantAgeRange;
        //    existed.RegistrantGender = model.RegistrantGender;
        //    existed.RegistrantAddress = model.RegistrantAddress ?? existed.RegistrantAddress;
        //    existed.ReferenceSource = model.ReferenceSource ?? existed.ReferenceSource;
        //    existed.EventId = model.EventId ?? existed.EventId;
        //    existed = await SetBaseEntityToUpdateFunc(existed) ;
        //    return await _repository.EventRegistrationRepository.Update(existed) ? (2, existed) //update thanh cong
        //                                                                  : (3, null);       //update fail
        //}
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
            return await _repository.EventRegistrationRepository.GetByID(id);
        }
        public async Task<List<EventRegistration>?> GetAllActiveEventRegistrations(Guid eventId)
        {
            var eventRegistrations = await _repository.EventRegistrationRepository.GetAll(eventId);

            return eventRegistrations.Count == 0 ? null : eventRegistrations;
        }
        public async Task<(List<object>, List<object>, List<object>, List<object>, List<object>)> Test (Guid eventId)
        {
            return await _repository.EventRegistrationRepository.GetStatistic(eventId);
        }
        public async void SendEmai(Ticket ticket)
        {
            var qrData = new
            {
                id = ticket.Id,
                ticketCode = ticket.TicketCode,
                eventId = ticket.EventRegistration.Event.EventName,
                registrationId = ticket.RegistrationId,
                attendeeName = ticket.EventRegistration.RegistrantName,
                attendeeEmail = ticket.EventRegistration.RegistrantEmail,
                attendeePhone = ticket.EventRegistration.RegistrantPhoneNumber,
                attendeeAddress = ticket.EventRegistration.RegistrantAddress,
                eventName = ticket.EventRegistration.Event.EventName,
                eventStartDate = ticket.EventRegistration.Event.StartDate,
                eventEndDate = ticket.EventRegistration.Event.EndDate,
                eventLocation = ticket.EventRegistration.Event.Zone.Street.Address,
                zoneId = ticket.EventRegistration.Event.ZoneId,
                zoneName = ticket.EventRegistration.Event.Zone.ZoneName,
                issuedAt = ticket.CreatedDate
            };
            string jsonData = JsonSerializer.Serialize(qrData);
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(jsonData, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(5);

            var barCodeInfor = ticket.Id + " " + ticket.TicketCode;
            Barcode barcode = new();
            barcode.Encode(BarcodeStandard.Type.Code128, barCodeInfor, 600, 200);
            string tempBarFilePath = Path.Combine(Path.GetTempPath(), "test-bar.png");
            barcode.SaveImage(tempBarFilePath, SaveTypes.Png);

            var from = new MailAddress(_smtpSettings.From);
            var to = new MailAddress(ticket.EventRegistration.RegistrantEmail);
            var htmlBody = await _razorTemplateEngine.RenderAsync("ResponseModel/EventRegistration.cshtml", ticket);

            var mail = new MailMessage(from, to)
            {
                Subject = "[SmartBookStreet] Thư cảm ơn",
                IsBodyHtml = true
            };
            string tempQRFilePath = Path.Combine(Path.GetTempPath(), "qrCode.png");
            qrCodeImage.Save(tempQRFilePath, ImageFormat.Png);
            var view = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
            var image = new LinkedResource(tempQRFilePath, MediaTypeNames.Image.Png)
            {
                ContentId = "qrImage",
                TransferEncoding = TransferEncoding.Base64
            };
            var image2 = new LinkedResource(tempBarFilePath, MediaTypeNames.Image.Png)
            {
                ContentId = "barImage",
                TransferEncoding = TransferEncoding.Base64
            };
            view.LinkedResources.Add(image);
            view.LinkedResources.Add(image2);
            mail.AlternateViews.Add(view);

            await _smtpClient.SendMailAsync(mail);
        }        
        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static int GenerateRandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max + 1);
        }
    }
}
