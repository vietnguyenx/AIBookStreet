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
    [Route("api/inventories")]
    [ApiController]
    
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly IMapper _mapper;

        public InventoryController(IInventoryService inventoryService, IMapper mapper)
        {
            _inventoryService = inventoryService;
            _mapper = mapper;
        }

        [HttpGet]
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

        [HttpGet("book/{bookId}")]
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

        [HttpGet("store/{storeId}")]
        public async Task<IActionResult> GetByStoreId(Guid storeId)
        {
            try
            {
                var inventories = await _inventoryService.GetByStoreId(storeId);
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

        [Authorize]
        [HttpPost]
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

        [Authorize]
        [HttpPatch("{idBook}/{idStore}")]
        public async Task<IActionResult> Delete(Guid idBook, Guid idStore)
        {
            try
            {
                if (idBook != Guid.Empty && idStore != Guid.Empty)
                {
                    var isInventory = await _inventoryService.Delete(idBook, idStore);

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

        [HttpPatch("scan")]
        public async Task<IActionResult> UpdateQuantityByBarcode([FromBody] BookScanRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.BookCode))
                {
                    return BadRequest(new BaseResponse(false, "Book code is required"));
                }

                if (request.StoreId == Guid.Empty)
                {
                    return BadRequest(new BaseResponse(false, "Store ID is required"));
                }

                if (request.Quantity <= 0)
                {
                    return BadRequest(new BaseResponse(false, "Quantity must be greater than zero"));
                }

                var (success, message) = await _inventoryService.UpdateQuantityByCode(request.BookCode, request.StoreId, request.Quantity);
                
                return success 
                    ? Ok(new BaseResponse(true, message)) 
                    : BadRequest(new BaseResponse(false, message));
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse(false, ex.Message));
            }
        }
    }
}
