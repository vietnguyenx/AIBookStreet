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
    [Route("api/user-stores")]
    [ApiController]
    public class UserStoreController : ControllerBase
    {
        private readonly IUserStoreService _userStoreService;
        private readonly IMapper _mapper;

        public UserStoreController(IUserStoreService userStoreService, IMapper mapper)
        {
            _userStoreService = userStoreService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var userStores = await _userStoreService.GetAll();

                return userStores switch
                {
                    null => Ok(new ItemListResponse<UserStoreModel>(ConstantMessage.Fail, null)),
                    not null => Ok(new ItemListResponse<UserStoreModel>(ConstantMessage.Success, userStores))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            try
            {
                var userStore = await _userStoreService.GetByUserId(userId);
                return userStore switch
                {
                    null => Ok(new ItemListResponse<UserStoreModel>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemListResponse<UserStoreModel>(ConstantMessage.Success, userStore))
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
                var userStore = await _userStoreService.GetByStoreId(storeId);
                return userStore switch
                {
                    null => Ok(new ItemListResponse<UserStoreModel>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemListResponse<UserStoreModel>(ConstantMessage.Success, userStore))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add([FromForm] UserStoreRequest userStoreRequest)
        {
            try
            {
                var userStoreModel = _mapper.Map<UserStoreModel>(userStoreRequest);
                userStoreModel.ContractFile = userStoreRequest.ContractFile;

                var (isSuccess, message) = await _userStoreService.Add(userStoreModel);
                return isSuccess switch
                {
                    true => Ok(new BaseResponse(isSuccess, message)),
                    false => BadRequest(new BaseResponse(isSuccess, message))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse(false, ex.Message));
            }
        }

        [Authorize]
        [HttpPatch("{idUser}/{idStore}")]
        public async Task<IActionResult> Delete(Guid idUser, Guid idStore)
        {
            try
            {
                if (idUser != Guid.Empty && idStore != Guid.Empty)
                {
                    var isUserStore = await _userStoreService.Delete(idUser, idStore);

                    return isUserStore switch
                    {
                        true => Ok(new BaseResponse(isUserStore, ConstantMessage.Success)),
                        _ => BadRequest(new BaseResponse(isUserStore, ConstantMessage.Fail))
                    };
                }
                else
                {
                    return BadRequest(new BaseResponse(false, "IDs must not be empty"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse(false, ex.Message));
            }
        }

        [Authorize]
        [HttpGet("download-contract/{userId}/{storeId}")]
        public async Task<IActionResult> DownloadContractFile(Guid userId, Guid storeId)
        {
            try
            {
                if (userId == Guid.Empty || storeId == Guid.Empty)
                {
                    return BadRequest(new BaseResponse(false, "User ID và Store ID không được để trống"));
                }

                var result = await _userStoreService.DownloadContractFile(userId, storeId);
                
                if (result == null)
                {
                    return NotFound(new BaseResponse(false, "Không tìm thấy file hợp đồng"));
                }

                var (fileData, contentType, fileName) = result.Value;
                
                return File(fileData, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse(false, ex.Message));
            }
        }

        [HttpPost("send-expiration-warnings")]
        public async Task<IActionResult> SendExpirationWarnings()
        {
            try
            {
                var (totalSent, message) = await _userStoreService.SendExpirationWarningEmails();
                return Ok(new BaseResponse(true, $"{message} Đã gửi {totalSent} email thông báo."));
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse(false, ex.Message));
            }
        }

        [HttpPost("test-contract-email")]
        public async Task<IActionResult> TestContractEmail([FromBody] ContractEmailRequest request)
        {
            try
            {
                var emailService = HttpContext.RequestServices.GetRequiredService<IUserAccountEmailService>();
                
                var emailModel = new ContractNotificationEmailModel
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    FullName = request.FullName,
                    Phone = request.Phone ?? "0123456789",
                    Address = request.Address ?? "123 Test Street",
                    
                    StoreName = request.StoreName,
                    StoreAddress = request.StoreAddress ?? "456 Store Street",
                    StoreType = request.StoreType ?? "BookStore",
                    
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Status = request.Status ?? "Active",
                    ContractNumber = request.ContractNumber ?? "HD-TEST-001",
                    ContractFileUrl = request.ContractFileUrl ?? "",
                    Notes = request.Notes ?? "Đây là email test",
                    
                    CreatedDate = DateTime.Now,
                    LoginUrl = "https://smart-book-street-next-aso3.vercel.app/login",
                    BaseImgUrl = request.BaseImgUrl ?? ""
                };

                var result = await emailService.SendContractNotificationEmailAsync(emailModel);
                
                return result 
                    ? Ok(new BaseResponse(true, "Email thông báo hợp đồng đã được gửi thành công"))
                    : BadRequest(new BaseResponse(false, "Không thể gửi email thông báo hợp đồng"));
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse(false, ex.Message));
            }
        }

        [HttpPost("test-expiration-email")]
        public async Task<IActionResult> TestExpirationEmail([FromBody] ExpirationEmailRequest request)
        {
            try
            {
                var emailService = HttpContext.RequestServices.GetRequiredService<IUserAccountEmailService>();
                
                var emailModel = new ContractExpirationEmailModel
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    FullName = request.FullName,
                    Phone = request.Phone ?? "0123456789",
                    Address = request.Address ?? "123 Test Street",
                    
                    StoreName = request.StoreName,
                    StoreAddress = request.StoreAddress ?? "456 Store Street",
                    StoreType = request.StoreType ?? "BookStore",
                    
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    DaysUntilExpiration = request.DaysUntilExpiration,
                    Status = request.Status ?? "Active",
                    ContractNumber = request.ContractNumber ?? "HD-TEST-001",
                    ContractFileUrl = request.ContractFileUrl ?? "",
                    Notes = request.Notes ?? "Đây là email test hết hạn",
                    
                    NotificationDate = DateTime.Now,
                    LoginUrl = "https://smart-book-street-next-aso3.vercel.app/login",
                    BaseImgUrl = request.BaseImgUrl ?? "",
                    ContactEmail = "support@aibookstreet.com",
                    ContactPhone = "1900-xxxx"
                };

                var result = await emailService.SendContractExpirationEmailAsync(emailModel);
                
                return result 
                    ? Ok(new BaseResponse(true, "Email thông báo hết hạn hợp đồng đã được gửi thành công"))
                    : BadRequest(new BaseResponse(false, "Không thể gửi email thông báo hết hạn hợp đồng"));
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse(false, ex.Message));
            }
        }
    }
}
