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
    [Route("api/user-roles")]
    [ApiController]
    
    public class UserRoleController : ControllerBase
    {
        private readonly IUserRoleService _userRoleService;
        private readonly IMapper _mapper;

        public UserRoleController(IUserRoleService userRoleService, IMapper mapper)
        {
            _userRoleService = userRoleService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var userRoles = await _userRoleService.GetAll();

                return userRoles switch
                {
                    null => Ok(new ItemListResponse<UserRoleModel>(ConstantMessage.Fail, null)),
                    not null => Ok(new ItemListResponse<UserRoleModel>(ConstantMessage.Success, userRoles))
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
                var userRole = await _userRoleService.GetByUserId(userId);
                return userRole switch
                {
                    null => Ok(new ItemListResponse<UserRoleModel>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemListResponse<UserRoleModel>(ConstantMessage.Success, userRole))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("role/{roleId}")]
        public async Task<IActionResult> GetByRoleId(Guid roleId)
        {
            try
            {
                var userRole = await _userRoleService.GetByRoleId(roleId);
                return userRole switch
                {
                    null => Ok(new ItemListResponse<UserRoleModel>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemListResponse<UserRoleModel>(ConstantMessage.Success, userRole))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add(UserRoleRequest userRoleRequest)
        {
            try
            {
                var isUserRole = await _userRoleService.Add(_mapper.Map<UserRoleModel>(userRoleRequest));

                return isUserRole switch
                {
                    true => Ok(new BaseResponse(isUserRole, ConstantMessage.Success)),
                    _ => Ok(new BaseResponse(isUserRole, ConstantMessage.Fail))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPatch("{idUser}/{idRole}")]
        public async Task<IActionResult> Delete(Guid idUser, Guid idRole)
        {
            try
            {
                if (idUser != Guid.Empty && idRole != Guid.Empty)
                {
                    var isUserRole = await _userRoleService.Delete(idUser, idRole);

                    return isUserRole switch
                    {
                        true => Ok(new BaseResponse(isUserRole, ConstantMessage.Success)),
                        _ => Ok(new BaseResponse(isUserRole, ConstantMessage.Fail))
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
        
        [Authorize]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingRoleRequests()
        {
            try
            {
                var pendingRoles = await _userRoleService.GetPendingRoleRequests();
                return pendingRoles switch
                {
                    null => Ok(new ItemListResponse<UserRoleModel>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemListResponse<UserRoleModel>(ConstantMessage.Success, pendingRoles))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPatch("approve/{userId}/{roleId}")]
        public async Task<IActionResult> ApproveRole(Guid userId, Guid roleId, [FromQuery] bool approve)
        {
            try
            {
                if (userId == Guid.Empty || roleId == Guid.Empty)
                {
                    return BadRequest("IDs must not be empty");
                }

                var result = await _userRoleService.ApproveRole(userId, roleId, approve);
                return result switch
                {
                    true => Ok(new BaseResponse(true, approve ? "Yêu cầu quyền đã được phê duyệt" : "Yêu cầu quyền đã bị từ chối")),
                    false => BadRequest(new BaseResponse(false, "Không tìm thấy yêu cầu quyền"))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
