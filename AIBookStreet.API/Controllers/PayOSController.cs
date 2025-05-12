using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.Services.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;

namespace AIBookStreet.API.Controllers
{
    [Route("api/payOS")]
    [ApiController]
    public class PayOSController (IPayOSService payOSService) : ControllerBase
    {
        private readonly IPayOSService _payOSService = payOSService;
        [AllowAnonymous]
        [HttpPost("")]
        public async Task<IActionResult> CreatePaymentLink(Guid orderId)
        {
            try
            {
                var result = await _payOSService.CreatePaymentLink(orderId);
                return result.Item1 switch
                {
                    0 => Ok(new BaseResponse(false, "ClientId not found")),
                    1 => Ok(new BaseResponse(false, "ApiKey not found")),
                    2 => Ok(new BaseResponse(false, "ChecksumKey not found")),
                    3 => Ok(new BaseResponse(false, "Order not found!!")),
                    _ => Ok(new ItemResponse<CreatePaymentResult>("Đã tạo link thanh toán", result.Item2)),
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{orderCode}")]
        public async Task<IActionResult> GetPaymentLinkInfomation([FromRoute] long orderCode)
        {
            try
            {
                var result = await _payOSService.GetPaymentLinkInformation(orderCode);
                return result.Item1 switch
                {
                    0 => Ok(new BaseResponse(false, "ClientId not found")),
                    1 => Ok(new BaseResponse(false, "ApiKey not found")),
                    2 => Ok(new BaseResponse(false, "ChecksumKey not found")),
                    3 => Ok(new BaseResponse(false, "Payment link not found!!")),
                    _ => Ok(new ItemResponse<PaymentLinkInformation>("Đã thấy thông tin", result.Item2)),
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[HttpPut("{orderCode}")]
        //public async Task<ActionResult<ResultModel>> CancelOrder([FromRoute] int orderCode)
        //{
        //    var result = await _payOSService.CancelOrder(orderCode);
        //    return Ok(result);
        //}
        [HttpPost("payos_transfer_handler")]
        public async Task<IActionResult> PayOSTransferHandler(WebhookType body)
        {
            try
            {
                var result = await _payOSService.VerifyPaymentWebhookData(body);
                return result.Item1 switch
                {
                    0 => Ok(new BaseResponse(false, "ClientId not found")),
                    1 => Ok(new BaseResponse(false, "ApiKey not found")),
                    2 => Ok(new BaseResponse(false, "ChecksumKey not found")),
                    3 => Ok(new BaseResponse(true, "Payment success")),
                    _ => Ok(new BaseResponse(false, "Payment fail"))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
