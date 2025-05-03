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

        [HttpGet("entity/{entityId}")]
        public async Task<IActionResult> GetByEntityId(Guid entityId)
        {
            try
            {
                var inventories = await _inventoryService.GetByEntityId(entityId);
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

        [HttpGet("store/{storeId}/books")]
        public async Task<IActionResult> GetBooksByStoreId(Guid storeId)
        {
            try
            {
                var inventories = await _inventoryService.GetBooksByStoreId(storeId);
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

        [HttpGet("store/{storeId}/souvenirs")]
        public async Task<IActionResult> GetSouvenirsByStoreId(Guid storeId)
        {
            try
            {
                var inventories = await _inventoryService.GetSouvenirsByStoreId(storeId);
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
                if (inventoryRequest.EntityId == null || inventoryRequest.EntityId == Guid.Empty)
                {
                    return BadRequest(new BaseResponse(false, "Entity ID is required"));
                }

                if (inventoryRequest.StoreId == null || inventoryRequest.StoreId == Guid.Empty)
                {
                    return BadRequest(new BaseResponse(false, "Store ID is required"));
                }

                if (inventoryRequest.Quantity < 0)
                {
                    return BadRequest(new BaseResponse(false, "Quantity cannot be negative"));
                }

                var (success, message) = await _inventoryService.Add(_mapper.Map<InventoryModel>(inventoryRequest));

                return success 
                    ? Ok(new BaseResponse(true, message)) 
                    : BadRequest(new BaseResponse(false, message));
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse(false, ex.Message));
            }
        }

        [Authorize]
        [HttpPut("{entityId}/{storeId}")]
        public async Task<IActionResult> Update(Guid entityId, Guid storeId, [FromForm] int quantity)
        {
            try
            {
                if (entityId == Guid.Empty)
                {
                    return BadRequest(new BaseResponse(false, "Entity ID is required"));
                }

                if (storeId == Guid.Empty)
                {
                    return BadRequest(new BaseResponse(false, "Store ID is required"));
                }

                if (quantity < 0)
                {
                    return BadRequest(new BaseResponse(false, "Quantity cannot be negative"));
                }

                var (success, message) = await _inventoryService.Update(
                    entityId, 
                    storeId, 
                    quantity);
                
                return success 
                    ? Ok(new BaseResponse(true, message)) 
                    : BadRequest(new BaseResponse(false, message));
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse(false, ex.Message));
            }
        }

        [Authorize]
        [HttpPatch("{idEntity}/{idStore}")]
        public async Task<IActionResult> Delete(Guid idEntity, Guid idStore)
        {
            try
            {
                if (idEntity != Guid.Empty && idStore != Guid.Empty)
                {
                    var isInventory = await _inventoryService.Delete(idEntity, idStore);

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
                if (string.IsNullOrEmpty(request.ISBN))
                {
                    return BadRequest(new BaseResponse(false, "Mã ISBN không được để trống"));
                }

                if (request.StoreId == Guid.Empty)
                {
                    return BadRequest(new BaseResponse(false, "Cửa hàng không được để trống"));
                }

                if (request.Quantity <= 0)
                {
                    return BadRequest(new BaseResponse(false, "Số lượng phải lớn hơn 0"));
                }

                var (success, message) = await _inventoryService.UpdateQuantityByISBN(request.ISBN, request.StoreId, request.Quantity);
                
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
