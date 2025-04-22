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

namespace AIBookStreet.Services.Services.Service
{
    public class QRGeneratorService(IFluentEmailFactory fluentEmailFactory, IRazorTemplateEngine razorTemplateEngine) : IQRGeneratorService
    {
        private readonly IFluentEmailFactory _fluentEmailFactory = fluentEmailFactory;
        private readonly IRazorTemplateEngine _razorTemplateEngine = razorTemplateEngine;
        public async Task<int> SendEmail(string email)
        {
            var evtRegistration = new Ticket {
                Id = Guid.NewGuid(),
                TicketCode = "123",
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
        string jsonData = JsonSerializer.Serialize("abc");
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(jsonData, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            using (MemoryStream ms = new())
            {
                //qrCodeImage.Save(ms, ImageFormat.Png);
                //ms.Position = 0;
                //ContentType contentType = new(MediaTypeNames.Image.Png);
                //LinkedResource qrCodeResource = new(ms, contentType);
                //qrCodeResource.ContentId = "qrCodeImage";

                var htmlBody = await _razorTemplateEngine.RenderAsync("ResponseModel/EventRegistration.cshtml", evtRegistration);
                string tempFilePath = Path.Combine(Path.GetTempPath(), "qrCode.png");
                qrCodeImage.Save(tempFilePath, ImageFormat.Png);
                                
                await _fluentEmailFactory.Create()
                    .To(email)
                    .Subject("[SmartBookStreet] Thư cảm ơn")                    
                    .Body(htmlBody, true)
                    .AttachFromFilename(tempFilePath, MediaTypeNames.Image.Png, "qrcode.png")
                    .SendAsync();
                
            }
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
    }
}
