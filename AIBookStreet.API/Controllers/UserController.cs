using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.API.SearchModel;
using AIBookStreet.Services.Common;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AIBookStreet.Services.Services.Service;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AIBookStreet.API.Controllers
{
    [Route("api/users")]
    [ApiController]
    
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly IMapper _mapper;

        public UserController(IUserService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var users = await _service.GetAll();

                return users switch
                {
                    null => Ok(new ItemListResponse<UserModel>(ConstantMessage.Fail, null)),
                    not null => Ok(new ItemListResponse<UserModel>(ConstantMessage.Success, users))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("paginated")]
        public async Task<IActionResult> GetAllPagination(PaginatedRequest paginatedRequest)
        {
            try
            {
                var users = await _service.GetAllPagination(paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField, paginatedRequest.SortOrder.Value);
                long totalOrigin = await _service.GetTotalCount();
                return users switch
                {
                    null => Ok(new PaginatedListResponse<UserModel>(ConstantMessage.NotFound)),
                    not null => Ok(new PaginatedListResponse<UserModel>(ConstantMessage.Success, users, totalOrigin, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("Id is empty");
                }
                var userModel = await _service.GetById(id);

                return userModel switch
                {
                    null => Ok(new ItemResponse<UserModel>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<UserModel>(ConstantMessage.Success, userModel))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }

        [HttpGet("{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            try
            {
                var userModel = await _service.GetUserByEmail(new UserModel { Email = email });

                return userModel switch
                {
                    null => Ok(new ItemResponse<UserModel>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<UserModel>(ConstantMessage.Success, userModel))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            };
        }

        [HttpPost("search/paginated")]
        public async Task<IActionResult> SearchPagination(PaginatedRequest<UserSearchRequest> paginatedRequest)
        {
            try
            {
                var user = _mapper.Map<UserModel>(paginatedRequest.Result);
                var users = await _service.SearchPagination(user, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField, paginatedRequest.SortOrder.Value);

                return users.Item1 switch
                {
                    null => Ok(new PaginatedListResponse<UserModel>(ConstantMessage.NotFound, users.Item1, users.Item2, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField, paginatedRequest.SortOrder)),
                    not null => Ok(new PaginatedListResponse<UserModel>(ConstantMessage.Success, users.Item1, users.Item2, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField, paginatedRequest.SortOrder))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchWithoutPagination(UserSearchRequest userSearchRequest)
        {
            try
            {
                var user = _mapper.Map<UserModel>(userSearchRequest);
                var users = await _service.SearchWithoutPagination(user);

                return users switch
                {
                    null => Ok(new { message = ConstantMessage.NotFound, data = users }),
                    not null => Ok(new { message = ConstantMessage.Success, data = users })
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add([FromForm] UserRequest userRequest)
        {
            try
            {
                var userModel = _mapper.Map<UserModel>(userRequest);
                userModel.MainImageFile = userRequest.MainImageFile;
                userModel.AdditionalImageFiles = userRequest.AdditionalImageFiles;



                var (result, message) = await _service.Add(userModel);
                return result switch
                {
                    null => StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, message)),
                    not null => StatusCode(ConstantHttpStatus.CREATED, new ItemResponse<UserModel>(message, result))
                };
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] UserRequest userRequest)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ConstantMessage.EmptyId));
                }

                var userModel = _mapper.Map<UserModel>(userRequest);
                userModel.Id = id;
                userModel.MainImageFile = userRequest.MainImageFile;
                userModel.AdditionalImageFiles = userRequest.AdditionalImageFiles;

                var (result, message) = await _service.Update(userModel);
                return result switch
                {
                    null => StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, message)),
                    not null => StatusCode(ConstantHttpStatus.OK, new ItemResponse<UserModel>(message, result))
                };
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            }
        }

        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ConstantMessage.EmptyId));
                }

                var (result, message) = await _service.Delete(id);
                return result switch
                {
                    null => StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, message)),
                    not null => StatusCode(ConstantHttpStatus.OK, new ItemResponse<UserModel>(message, result))
                };
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthModel authModel)
        {
            try
            {
                var userModel = await _service.Login(authModel);

                if (userModel == null)
                {
                    return Ok(new LoginResponse<UserModel>(null, null, null, ConstantMessage.Fail));
                }

                JwtSecurityToken token = _service.CreateToken(userModel);

                return Ok(new LoginResponse<UserModel>(ConstantMessage.Success, userModel, new JwtSecurityTokenHandler().WriteToken(token)
                , token.ValidTo.ToString()));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/<AuthController>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRequest userRequest)
        {
            try
            {
                UserModel _userModel = await _service.GetUserByEmailOrUsername(_mapper.Map<UserModel>(userRequest));

                if (_userModel != null)
                {
                    return Ok(new ItemResponse<UserModel>(ConstantMessage.Duplicate));
                }

                UserModel userModelMapping = _mapper.Map<UserModel>(userRequest);
                userModelMapping.Password = userRequest.Password;

                UserModel userModel = await _service.Register(userModelMapping);

                return userModel switch
                {
                    null => Ok(new ItemResponse<UserModel>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<UserModel>(ConstantMessage.Success, userModel))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var user = await _service.GetUserInfo();
                if (user == null)
                {
                    return Ok(new ItemResponse<UserModel>(ConstantMessage.NotFound));
                }

                var userModel = _mapper.Map<UserModel>(user);
                return Ok(new ItemResponse<UserModel>(ConstantMessage.Success, userModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("login-google")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse", "User"),
                Items =
                {
                    { "returnUrl", "/" }
                }
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            try
            {
                var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                if (!authenticateResult.Succeeded)
                    return BadRequest(new BaseResponse(false, "Đăng nhập Google thất bại"));

                var userModel = await _service.ProcessGoogleLoginAsync(authenticateResult.Principal);
                if (userModel == null)
                    return BadRequest(new BaseResponse(false, "Không thể xử lý thông tin người dùng từ Google"));

                JwtSecurityToken token = _service.CreateToken(userModel);
                string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(7)
                };
                
                Response.Cookies.Append("auth_token", jwtToken, cookieOptions);
                
                var frontendUrl = "http://localhost:3000";
                return Redirect(frontendUrl);
                //return Redirect("/google-login-test.html?loginSuccess=true");
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse(false, ex.Message));
            }
        }
    }
}
