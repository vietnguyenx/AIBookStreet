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

namespace AIBookStreet.Services.Services.Service
{
    public class QRGeneratorService(IFluentEmailFactory fluentEmailFactory, IRazorTemplateEngine razorTemplateEngine) : IQRGeneratorService
    {
        private readonly IFluentEmailFactory _fluentEmailFactory = fluentEmailFactory;
        private readonly IRazorTemplateEngine _razorTemplateEngine = razorTemplateEngine;
        public async Task<int> SendEmail(string email)
        {
            var evtRegistration = new EventRegistration { 
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
                    IsOpen = true,
                    AllowAds = true,
                    CreatedDate = DateTime.Now,
                    IsDeleted = false,
                }
            };
            string jsonData = JsonSerializer.Serialize(evtRegistration);
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
                    .AttachFromFilename(tempFilePath, MediaTypeNames.Image.Png, "QRCode.png")
                    .SendAsync();
                File.Delete(tempFilePath);
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
