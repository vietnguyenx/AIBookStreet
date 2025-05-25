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
        //private readonly IQRGeneratorService _service = service;
        
        //[AllowAnonymous]
        //[HttpGet]
        //public async Task<IActionResult> Get(string email)
        //{
        //    try
        //    {
        //        var result = await _service.ExportListToExcel(email);
        //        return Ok(result);
        //    } catch
        //    {
        //        throw;
        //    }
        //}
    }
}
