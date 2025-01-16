using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.API.Tool.Constant;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AIBookStreet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController(IImageService service, IMapper mapper) : ControllerBase
    {
        private readonly IImageService _service = service;
        private readonly IMapper _mapper = mapper;

        //[HttpPost("upload")]
        //public async Task<IActionResult> TestUpload(List<IFormFile> files)
        //{
        //    try
        //    {
        //        var result = await _service.UploadImagesAsync(files);
        //        return result == null ? Ok(new BaseResponse(false, "Đã xảy ra lỗi!!!")) : Ok(new ItemListResponse<string>(ConstantMessage.Success, result));
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
        //[Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddImages(List<ImageModel> models)
        {
            try
            {
                var result = await _service.AddImages(models);
                return result == null ? Ok(new BaseResponse(false, "Đã xảy ra lỗi!!!")) : Ok(new ItemListResponse<ImageRequest>(ConstantMessage.Success, _mapper.Map<List<ImageRequest>>(result)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateAnImage([FromRoute] Guid id, [FromQuery] string altText)
        {
            try
            {
                var result = await _service.UpdateAnImage(id, altText);

                return result.Item1 switch
                {
                    1 => Ok(new BaseResponse(false, "Không tồn tại!!!")),
                    2 => Ok(new BaseResponse(true, "Đã cập nhật thông tin!")),
                    _ => Ok(new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại"))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[Authorize]
        [HttpPut("delete/{id}")]
        public async Task<IActionResult> DeleteAnImage([FromRoute] Guid id)
        {
            try
            {
                var result = await _service.DeleteAnImage(id);

                return result.Item1 switch
                {
                    1 => Ok(new BaseResponse(false, "Không tồn tại!!!")),
                    2 => Ok(new BaseResponse(true, "Đã xóa thành công!")),
                    _ => Ok(new BaseResponse(false, "Đã xảy ra lỗi, vui lòng kiểm tra lại!!!"))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetAnImageById([FromRoute] Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("Id is empty");
                }
                var image = await _service.GetAnImageById(id);

                return image switch
                {
                    null => Ok(new ItemResponse<Image>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<Image>(ConstantMessage.Success, image))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [HttpGet("get-by-type-and-entityID")]
        public async Task<IActionResult> GetList([FromQuery]string? type, Guid? entityID)
        {
            try
            {
                var images = await _service.GetImagesByTypeAndEntityID(type, entityID);

                return images switch
                {
                    null => Ok(new ItemListResponse<ImageRequest>(ConstantMessage.Success, null)),
                    not null => Ok(new ItemListResponse<ImageRequest>(ConstantMessage.Success, _mapper.Map<List<ImageRequest>>(images)))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
