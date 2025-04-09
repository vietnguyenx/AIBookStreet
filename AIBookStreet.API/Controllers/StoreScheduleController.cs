using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIBookStreet.API.Controllers
{
    [Route("api/store-schedules")]
    [ApiController]
    public class StoreScheduleController : ControllerBase
    {
        private readonly IStoreScheduleService _storeScheduleService;

        public StoreScheduleController(IStoreScheduleService storeScheduleService)
        {
            _storeScheduleService = storeScheduleService;
        }

        [HttpGet]
        public async Task<ActionResult<List<StoreScheduleModel>>> GetAll()
        {
            var schedules = await _storeScheduleService.GetAll();
            if (schedules == null)
                return NotFound();
            return Ok(schedules);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StoreScheduleModel>> GetById(Guid id)
        {
            var schedule = await _storeScheduleService.GetById(id);
            if (schedule == null)
                return NotFound();
            return Ok(schedule);
        }

        [HttpGet("store/{storeId}")]
        public async Task<ActionResult<List<StoreScheduleModel>>> GetByStoreId(Guid storeId)
        {
            var schedules = await _storeScheduleService.GetByStoreId(storeId);
            if (schedules == null)
                return NotFound();
            return Ok(schedules);
        }

        [HttpGet("store/{storeId}/day/{dayOfWeek}")]
        public async Task<ActionResult<StoreScheduleModel>> GetByStoreIdAndDayOfWeek(Guid storeId, DayOfWeek dayOfWeek)
        {
            var schedule = await _storeScheduleService.GetByStoreIdAndDayOfWeek(storeId, dayOfWeek);
            if (schedule == null)
                return NotFound();
            return Ok(schedule);
        }

        [HttpGet("store/{storeId}/special-date")]
        public async Task<ActionResult<StoreScheduleModel>> GetByStoreIdAndSpecialDate(Guid storeId, [FromQuery] DateTime specialDate)
        {
            var schedule = await _storeScheduleService.GetByStoreIdAndSpecialDate(storeId, specialDate);
            if (schedule == null)
                return NotFound();
            return Ok(schedule);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<StoreScheduleModel>> Add([FromBody] StoreScheduleModel model)
        {
            var (result, message) = await _storeScheduleService.Add(model);
            if (result == null)
                return BadRequest(message);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<StoreScheduleModel>> Update(Guid id, [FromBody] StoreScheduleModel model)
        {
            if (id != model.Id)
                return BadRequest("ID không khớp");

            var (result, message) = await _storeScheduleService.Update(model);
            if (result == null)
                return BadRequest(message);
            return Ok(result);
        }

        [Authorize]
        [HttpPatch("{id}")]
        public async Task<ActionResult<StoreScheduleModel>> Delete(Guid id)
        {
            var (result, message) = await _storeScheduleService.Delete(id);
            if (result == null)
                return BadRequest(message);
            return Ok(result);
        }
    }
} 