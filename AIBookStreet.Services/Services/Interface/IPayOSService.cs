using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IPayOSService
    {
        Task<(long, CreatePaymentResult?)> CreatePaymentLink(Guid orderId);
        Task<(long, PaymentLinkInformation?)> GetPaymentLinkInformation(long orderCode);
        Task<(long, PaymentLinkInformation?)> CancelOrder(int orderCode);
        Task<(long, int?)> VerifyPaymentWebhookData(WebhookType body);
    }
}
