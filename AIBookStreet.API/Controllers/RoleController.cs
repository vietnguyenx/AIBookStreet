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
    [Route("api/roles")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly IMapper _mapper;

        public RoleController(IRoleService roleService, IMapper mapper)
        {
            _roleService = roleService;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var roles = await _roleService.GetAll();

                return roles switch
                {
                    null => Ok(new ItemListResponse<RoleModel>(ConstantMessage.Fail, null)),
                    not null => Ok(new ItemListResponse<RoleModel>(ConstantMessage.Success, roles))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add(RoleRequest roleRequest)
        {
            try
            {
                var isRole = await _roleService.Add(_mapper.Map<RoleModel>(roleRequest));

                return isRole switch
                {
                    true => Ok(new BaseResponse(isRole, ConstantMessage.Success)),
                    _ => Ok(new BaseResponse(isRole, ConstantMessage.Fail))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (id != Guid.Empty)
                {
                    var isRole = await _roleService.Delete(id);

                    return isRole switch
                    {
                        true => Ok(new BaseResponse(isRole, ConstantMessage.Success)),
                        _ => Ok(new BaseResponse(isRole, ConstantMessage.Fail))
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
