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
    public class EventController(IEventService service, IMapper mapper) : ControllerBase
    {
        private readonly IEventService _service = service;
        private readonly IMapper _mapper = mapper;

        //[Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddAnEvent([FromQuery] EventModel model)
        {
            try
            {
                var result = await _service.AddAnEvent(model);
                return result.Item1 == 1 ? Ok(new BaseResponse(false, "Đã có sự kiện trong thời gian trên đường sách này")) 
                     : result.Item1 == 2 ? Ok(new BaseResponse(true, "Đã thêm"))
                     :                     Ok(new BaseResponse(false, "Đã xảy ra lỗi"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateAnEvent([FromRoute] Guid id, [FromQuery] EventModel model)
        {
            try
            {
                var result = await _service.UpdateAnEvent(id, model);

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
        public async Task<IActionResult> DeleteAnEvent([FromRoute] Guid id)
        {
            try
            {
                var result = await _service.DeleteAnEvent(id);

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
        public async Task<IActionResult> GetAnEventById([FromRoute] Guid id)
        {
            try
            {
                var evt = await _service.GetAnEventById(id);

                return evt switch
                {
                    null => Ok(new ItemResponse<Event>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<Event>(ConstantMessage.Success, evt))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
        [HttpGet("get-event-coming")]
        public async Task<IActionResult> GetEventsComing([FromQuery]int number)
        {
            try
            {
                var events = await _service.GetEventComing(number);

                return events switch
                {
                    null => Ok(new ItemListResponse<EventRequest>(ConstantMessage.Success, null)),
                    not null => Ok(new ItemListResponse<EventRequest>(ConstantMessage.Success, _mapper.Map<List<EventRequest>>(events)))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("pagination-and-search")]
        public async Task<IActionResult> GetAllEventsPagination(string? key, DateTime? start, DateTime? end, Guid? streetID, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            try
            {
                var events = await _service.GetAllEventsPagination(key, start, end, streetID, pageNumber, pageSize, sortField, desc);

                return events.Item2 switch
                {
                    0 => Ok(new PaginatedListResponse<EventRequest>(ConstantMessage.Success, null)),
                    _ => Ok(new PaginatedListResponse<EventRequest>(ConstantMessage.Success, _mapper.Map<List<EventRequest>>(events.Item1), events.Item2, pageNumber != null ? (int)pageNumber : 1, pageSize != null ? (int)pageSize : 10, sortField, desc != null && (desc != false) ? 0 : 1))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }
    }
}
