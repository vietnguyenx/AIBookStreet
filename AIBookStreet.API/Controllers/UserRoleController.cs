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
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class UserRoleController : ControllerBase
    {
        private readonly IUserRoleService _userRoleService;
        private readonly IMapper _mapper;

        public UserRoleController(IUserRoleService userRoleService, IMapper mapper)
        {
            _userRoleService = userRoleService;
            _mapper = mapper;
        }

        [HttpGet("get-all")]
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

        [HttpGet("get-by-user/{userId}")]
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

        [HttpGet("get-by-role/{roleId}")]
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

        [HttpPost("add")]
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

        [HttpPut("delete")]
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
