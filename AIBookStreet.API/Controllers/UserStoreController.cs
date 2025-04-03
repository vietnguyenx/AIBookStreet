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

        [HttpGet("{userId}")]
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

        [HttpGet("{storeId}")]
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
        public async Task<IActionResult> Add(UserStoreRequest userStoreRequest)
        {
            try
            {
                var isUserStore = await _userStoreService.Add(_mapper.Map<UserStoreModel>(userStoreRequest));

                return isUserStore switch
                {
                    true => Ok(new BaseResponse(isUserStore, ConstantMessage.Success)),
                    _ => Ok(new BaseResponse(isUserStore, ConstantMessage.Fail))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
                        _ => Ok(new BaseResponse(isUserStore, ConstantMessage.Fail))
                    };
                }
                else
                {
                    return BadRequest("IDs must not be empty");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
