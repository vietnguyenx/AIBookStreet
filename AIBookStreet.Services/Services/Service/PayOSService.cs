using AIBookStreet.Repositories.Repositories.UnitOfWork.Interface;
using AIBookStreet.Services.Services.Interface;
using Microsoft.Extensions.Configuration;
using Net.payOS.Types;
using Net.payOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AIBookStreet.Repositories.Data.Entities;

namespace AIBookStreet.Services.Services.Service
{
    public class PayOSService(IConfiguration configuration, IUnitOfWork unitOfWork) : IPayOSService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<(long, CreatePaymentResult?)> CreatePaymentLink(Guid orderId)
        {
            var clientId = _configuration["payOS:ClientId"];
            if (clientId == null)
            {
                return (0, null); //ClientId not found
            }
            var apiKey = _configuration["payOS:ApiKey"];
            if (apiKey == null)
            {
                return (1, null); //ApiKey not found
            }
            var checksumKey = _configuration["payOS:ChecksumKey"];
            if (checksumKey == null)
            {
                return (2, null); //ChecksumKey not found
            }

            PayOS _payOS = new(
                clientId,
                apiKey,
                checksumKey
            );

            var order = await _unitOfWork.OrderRepository.GetByID(orderId);
            if (order is null)
            {
                return (3, null); //Order not found!!
            }            
            var orders = await _unitOfWork.OrderRepository.GetAll();
            int orderCode = orders.Count;

            var orderDetails = await _unitOfWork.OrderDetailRepository.GetAllOrderDetail(null, orderId, null, null);
            List<ItemData> items = [];
            if (orderDetails is not null)
            {
                foreach (var orderDetail in orderDetails)
                {
                    var inventory = await _unitOfWork.InventoryRepository.GetByID(orderDetail.InventoryId);
                    if (inventory != null && inventory.Book != null)
                    {
                        var price = (int)inventory.Book.Price * orderDetail.Quantity;
                        items.Add(new ItemData(inventory.Book.Title, orderDetail.Quantity, price));
                    }
                    if (inventory != null && inventory.Souvenir != null)
                    {
                        var price = (int)inventory.Souvenir.Price * orderDetail.Quantity;
                        items.Add(new ItemData(inventory.Souvenir.SouvenirName, orderDetail.Quantity, price));
                    }

                }
            }
            long expiredAt = (long)(DateTime.UtcNow.AddMinutes(10) - new DateTime(1970, 1, 1)).TotalSeconds;

            PaymentData paymentData = new(
                orderCode: orderCode,
                amount: (int)order.TotalAmount,
                description: "Thanh toan hoa don",
                items: items,
                cancelUrl: "fail",
                returnUrl: "https://smart-book-street-next-aso3.vercel.app",
                expiredAt: expiredAt
            );

            CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

            return  (4, createPayment);
        }
        public async Task<(long, PaymentLinkInformation?)> GetPaymentLinkInformation(long orderCode)
        {
            var clientId = _configuration["payOS:ClientId"];
            if (clientId == null)
            {
                return (0, null); //ClientId not found
            }
            var apiKey = _configuration["payOS:ApiKey"];
            if (apiKey == null)
            {
                return (1, null); //ApiKey not found
            }
            var checksumKey = _configuration["payOS:ChecksumKey"];
            if (checksumKey == null)
            {
                return (2, null); //ChecksumKey not found
            }

            PayOS _payOS = new(clientId, apiKey, checksumKey);
            PaymentLinkInformation paymentLinkInformation = await _payOS.getPaymentLinkInformation(orderCode);
            return paymentLinkInformation == null ?
                (3, null) //not found
                : (4, paymentLinkInformation) ;
        }
        public async Task<(long, PaymentLinkInformation?)> CancelOrder(int orderCode)
        {
            var clientId = _configuration["payOS:ClientId"];
            if (clientId == null)
            {
                return (0, null); //ClientId not found
            }
            var apiKey = _configuration["payOS:ApiKey"];
            if (apiKey == null)
            {
                return (1, null); //ApiKey not found
            }
            var checksumKey = _configuration["payOS:ChecksumKey"];
            if (checksumKey == null)
            {
                return (2, null); //ChecksumKey not found
            }

            PayOS _payOS = new(clientId, apiKey, checksumKey);
            var getPaymentLinkInformation = await _payOS.getPaymentLinkInformation((long)orderCode);
            if (getPaymentLinkInformation == null)
            {
                return (3, null); //not found
            }
            PaymentLinkInformation paymentLinkInformation = await _payOS.cancelPaymentLink(orderCode);

            return (4, paymentLinkInformation);
        }
        public (long, int?) VerifyPaymentWebhookData(WebhookType body)
        {
            try
            {
                var clientId = _configuration["payOS:ClientId"];
                if (clientId == null)
                {
                    return (0, null); //ClientId not found
                }
                var apiKey = _configuration["payOS:ApiKey"];
                if (apiKey == null)
                {
                    return (1, null); //ApiKey not found
                }
                var checksumKey = _configuration["payOS:ChecksumKey"];
                if (checksumKey == null)
                {
                    return (2, null); //ChecksumKey not found
                }

                PayOS _payOS = new(clientId, apiKey, checksumKey);
                WebhookData data = _payOS.verifyPaymentWebhookData(body);

                string responseCode = data.code;

                if (responseCode == "00")
                {
                    return (3, 0); // "Payment success"
                }
                return (4, 1); // "Payment failed"
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return (4, -1); // "Payment failed" };
            }
        }
    }
}
