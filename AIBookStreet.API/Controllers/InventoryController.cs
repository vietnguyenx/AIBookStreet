using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.API.Tool.Constant;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AIBookStreet.Services.Services.Service;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AIBookStreet.API.Controllers
{
    [Route("api/inventory")]
    [ApiController]
    //[Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly IMapper _mapper;

        public InventoryController(IInventoryService inventoryService, IMapper mapper)
        {
            _inventoryService = inventoryService;
            _mapper = mapper;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var inventories = await _inventoryService.GetAll();

                return inventories switch
                {
                    null => Ok(new ItemListResponse<InventoryModel>(ConstantMessage.Fail, null)),
                    not null => Ok(new ItemListResponse<InventoryModel>(ConstantMessage.Success, inventories))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-by-book/{bookId}")]
        public async Task<IActionResult> GetByBookId(Guid bookId)
        {
            try
            {
                var inventories = await _inventoryService.GetByBookId(bookId);
                return inventories switch
                {
                    null => Ok(new ItemListResponse<InventoryModel>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemListResponse<InventoryModel>(ConstantMessage.Success, inventories))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-by-bookstore/{bookStoreId}")]
        public async Task<IActionResult> GetByBookStoreId(Guid bookStoreId)
        {
            try
            {
                var inventories = await _inventoryService.GetByBookStoreId(bookStoreId);
                return inventories switch
                {
                    null => Ok(new ItemListResponse<InventoryModel>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemListResponse<InventoryModel>(ConstantMessage.Success, inventories))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add(InventoryRequest inventoryRequest)
        {
            try
            {
                var isInventory = await _inventoryService.Add(_mapper.Map<InventoryModel>(inventoryRequest));

                return isInventory switch
                {
                    true => Ok(new BaseResponse(isInventory, ConstantMessage.Success)),
                    _ => Ok(new BaseResponse(isInventory, ConstantMessage.Fail))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("delete")]
        public async Task<IActionResult> Delete(Guid idBook, Guid idBookStore)
        {
            try
            {
                if (idBook != Guid.Empty && idBookStore != Guid.Empty)
                {
                    var isInventory = await _inventoryService.Delete(idBook, idBookStore);

                    return isInventory switch
                    {
                        true => Ok(new BaseResponse(isInventory, ConstantMessage.Success)),
                        _ => Ok(new BaseResponse(isInventory, ConstantMessage.Fail))
                    };
                }
                else
                {
                    return BadRequest("It's not empty");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
