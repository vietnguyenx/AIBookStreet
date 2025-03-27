using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.API.SearchModel;
using AIBookStreet.API.Tool.Constant;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AIBookStreet.API.Controllers
{
    [Route("api/images")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _service;
        private readonly IMapper _mapper;

        public ImageController(IImageService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Add one or multiple images
        /// </summary>
        [Authorize]
        [HttpPost("")]
        public async Task<IActionResult> AddImages(
            [FromForm] List<IFormFile> files,
            [FromForm] string? type,
            [FromForm] string? altText,
            [FromForm] Guid? entityId)
        {
            try
            {
                var models = files.Select(file => new FileModel
                {
                    File = file,
                    Type = type,
                    AltText = altText,
                    EntityId = entityId
                }).ToList();

                var result = await _service.AddImages(models);
                return result == null 
                    ? Ok(new BaseResponse(false, "Đã xảy ra lỗi!!!")) 
                    : Ok(new ItemListResponse<ImageRequest>(ConstantMessage.Success, _mapper.Map<List<ImageRequest>>(result)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update an image
        /// </summary>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAnImage(
            [FromRoute] Guid id,
            [FromForm] UpdateImageRequest request)
        {
            try
            {
                var model = new FileModel
                {
                    File = request.File,
                    Type = request.Type,
                    AltText = request.AltText,
                    EntityId = request.EntityId
                };

                var result = await _service.UpdateAnImage(id, model);

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

        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> DeleteAnImage([FromRoute]Guid id)
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

        [AllowAnonymous]
        [HttpGet("{id}")]
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
            }
        }

        [AllowAnonymous]
        [HttpPost("get-by-type-or-entityID")]
        public async Task<IActionResult> GetList(ImageSearchRequest request)
        {
            try
            {
                var images = await _service.GetImagesByTypeAndEntityID(request.Type, request.EntityId);

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

        /// <summary>
        /// Get all images
        /// </summary>
        [AllowAnonymous]
        [HttpGet("list")]
        public async Task<IActionResult> GetAllImages()
        {
            try
            {
                var images = await _service.GetAllImages();
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
