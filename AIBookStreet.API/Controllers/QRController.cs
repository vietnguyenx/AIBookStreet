using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIBookStreet.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class QRController(IQRGeneratorService service) : ControllerBase
    {
        private readonly IQRGeneratorService _service = service;
        [AllowAnonymous]
        [HttpPost("send-email")]
        public async Task<IActionResult> AddAnAuthor([FromForm] string email)
        {
            try
            {
                var result = await _service.SendEmail(email);
                return result == 1 ? Ok(new BaseResponse(true, "Đã gửi")) : Ok(new BaseResponse(false, "Đã xảy ra lỗi!!!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpPost("create-qr-code")]
        public IActionResult AddAnAuthor([FromForm] string name, int age)
        {
            try
            {
                var result = _service.GenerateQRCode(name, age);
                return result == 1 ? Ok(new BaseResponse(true, "Đã tạo")) : Ok(new BaseResponse(false, "Đã xảy ra lỗi!!!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpPost("create-bar-code")]
        public IActionResult Mt([FromForm] string name)
        {
            try
            {
                var result = _service.GenerateBarCode(name);
                return result == 1 ? Ok(new BaseResponse(true, "Đã tạo")) : Ok(new BaseResponse(false, "Đã xảy ra lỗi!!!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
