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

namespace AIBookStreet.Services.Services.Service
{
    public class QRGeneratorService(SmtpClient smtpClient, IOptions<SmtpSettings> smtpSettings, IRazorTemplateEngine razorTemplateEngine) : IQRGeneratorService
    {
        private readonly SmtpClient _smtpClient = smtpClient;
        private readonly SmtpSettings _smtpSettings = smtpSettings.Value;
        private readonly IRazorTemplateEngine _razorTemplateEngine = razorTemplateEngine;
        public async Task<int> SendEmail(string email)
        {
            var evtRegistration = new Ticket {
                Id = Guid.NewGuid(),
                TicketCode = "123456",
                SecretPasscode = "789456",
                EventRegistration = new EventRegistration {
                    Id = Guid.NewGuid(),
                    RegistrantName = "Hiep",
                    RegistrantGender = "Nam",
                    RegistrantAgeRange = "18-25",
                    RegistrantEmail = email,
                    RegistrantPhoneNumber = "0983637752",
                    RegistrantAddress = "Bình Dương",
                    Event = new Event
                    {
                        Id = Guid.NewGuid(),
                        EventName = "Sự kiện mẫu",
                        StartDate = DateTime.Parse("2025-04-22 10:00:00"),
                        IsOpen = true,
                        AllowAds = true,
                        CreatedDate = DateTime.Now,
                        IsDeleted = false,
                        Zone = new Zone {
                            ZoneName = "Zone A",
                            Street = new Street
                            {
                                Address = "Đường sách Hồ Chí Minh, Đường Nguyễn Văn Bình, Phường Bến Nghé, Quận 1, TP.HCM"
                            }
                        }
                    }
                }
            };
            var qrData = new
            {
                id = evtRegistration.Id,
                ticketCode = evtRegistration.TicketCode,
                eventId = evtRegistration.EventRegistration.Event.EventName,
                registrationId = evtRegistration.RegistrationId,
                attendeeName = evtRegistration.EventRegistration.RegistrantName,
                attendeeEmail = evtRegistration.EventRegistration.RegistrantEmail,
                attendeePhone = evtRegistration.EventRegistration.RegistrantPhoneNumber,
                attendeeAddress = evtRegistration.EventRegistration.RegistrantAddress,
                eventName = evtRegistration.EventRegistration.Event.EventName,
                eventStartDate = evtRegistration.EventRegistration.Event.StartDate,
                eventEndDate = evtRegistration.EventRegistration.Event.EndDate,
                eventLocation = evtRegistration.EventRegistration.Event.Zone.Street.Address,
                zoneId = evtRegistration.EventRegistration.Event.ZoneId,
                zoneName = evtRegistration.EventRegistration.Event.Zone.ZoneName,
                issuedAt = evtRegistration.CreatedDate
            };
        string jsonData = JsonSerializer.Serialize(qrData);
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(jsonData, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(5);
            //using (MemoryStream ms = new())
            //{
            //    var htmlBody = await _razorTemplateEngine.RenderAsync("ResponseModel/EventRegistration.cshtml", evtRegistration);
            //    string tempFilePath = Path.Combine(Path.GetTempPath(), "qrCode.png");
            //    qrCodeImage.Save(tempFilePath, ImageFormat.Png);

            //    await _fluentEmailFactory.Create()
            //        .To(email)
            //        .Subject("[SmartBookStreet] Thư cảm ơn")                    
            //        .Body(htmlBody, true)
            //        .AttachFromFilename(tempFilePath, MediaTypeNames.Image.Png, "qrcode.png")
            //        .SendAsync();

            //}

            var barCodeInfor = evtRegistration.Id + " " + evtRegistration.TicketCode;
            Barcode barcode = new();
            barcode.Encode(BarcodeStandard.Type.Code128, barCodeInfor, 600, 200);
            using MemoryStream ms1 = new();
            barcode.SaveImage(ms1, SaveTypes.Png);
            ms1.Position = 0;
            //string tempBarFilePath = Path.Combine(Path.GetTempPath(), "test-bar.png");
            //barcode.SaveImage(tempBarFilePath, SaveTypes.Png);

            var from = new MailAddress(_smtpSettings.From);
            var to = new MailAddress(email);
            var htmlBody = await _razorTemplateEngine.RenderAsync("ResponseModel/EventRegistration.cshtml", evtRegistration);

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
        }
        public int GenerateQRCode(string name, int age)
        {
            object data = new
            {
                Name = name,
                Age = age
            };
            string jsonData = JsonSerializer.Serialize(data);
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(jsonData, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            string savePath = @"D:/test-qr.png";
            string directoryPath = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            qrCodeImage.Save(savePath, ImageFormat.Png);
            return 1;
        }
        public int GenerateBarCode(string infor)
        {
            Barcode barcode = new();
            barcode.Encode(BarcodeStandard.Type.Code128, infor, 600, 3200);
            string savePath = @"D:/test-bar.png";
            barcode.SaveImage(savePath, SaveTypes.Png);
            return 1;
        }
    }
}
