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
using System.Linq;

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

        /// <summary>
        /// Lọc bỏ những userRole chưa được phê duyệt khỏi UserModel
        /// </summary>
        private void FilterUnapprovedUserRoles(UserModel userModel)
        {
            if (userModel?.UserRoles != null && userModel.UserRoles.Any())
            {
                userModel.UserRoles = userModel.UserRoles.Where(ur => ur.IsApproved).ToList();
            }
        }

        /// <summary>
        /// Lọc bỏ những userRole chưa được phê duyệt khỏi danh sách UserModel
        /// </summary>
        private void FilterUnapprovedUserRoles(List<UserModel> userModels)
        {
            if (userModels != null)
            {
                foreach (var userModel in userModels)
                {
                    FilterUnapprovedUserRoles(userModel);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var users = await _service.GetAll();

                if (users != null)
                {
                    FilterUnapprovedUserRoles(users);
                }

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

                if (userModel != null)
                {
                    // Lọc bỏ những userRole chưa được phê duyệt
                    FilterUnapprovedUserRoles(userModel);
                }

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

        [HttpGet("by-email/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            try
            {
                var userModel = await _service.GetUserByEmail(new UserModel { Email = email });

                if (userModel != null)
                {
                    // Lọc bỏ những userRole chưa được phê duyệt
                    FilterUnapprovedUserRoles(userModel);
                }

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

                if (users != null)
                {
                    FilterUnapprovedUserRoles(users);
                }

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
                userModel.Password = userRequest.Password;

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
                var (userModel, needsPasswordChange, message) = await _service.Login(authModel);

                // User needs to change password
                if (needsPasswordChange)
                {
                    return StatusCode(403, new BaseResponse(false, message));
                }

                // Login failed
                if (userModel == null)
                {
                    return Unauthorized(new BaseResponse(false, ConstantMessage.Fail));
                }

                // Login successful
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
                Console.WriteLine("Register endpoint called with data: " + 
                    (userRequest != null ? $"Username={userRequest.UserName}, Email={userRequest.Email}" : "null"));
                
                // Validate input
                if (userRequest == null)
                {
                    Console.WriteLine("Registration failed: UserRequest is null");
                    return BadRequest(new BaseResponse(false, "Thông tin người dùng không được để trống"));
                }
                
                if (string.IsNullOrEmpty(userRequest.UserName) || string.IsNullOrEmpty(userRequest.Password) || string.IsNullOrEmpty(userRequest.Email))
                {
                    Console.WriteLine($"Registration failed: Username, password, or email missing");
                    return BadRequest(new BaseResponse(false, "Tên đăng nhập, mật khẩu và email là bắt buộc"));
                }

                var userModel = _mapper.Map<UserModel>(userRequest);
                var (result, message) = await _service.Register(userModel);

                if (result == null)
                {
                    return BadRequest(new BaseResponse(false, message));
                }

                return Ok(new ItemResponse<UserModel>(message, result));
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse(false, ex.Message));
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
                
                // Lọc bỏ những userRole chưa được phê duyệt
                FilterUnapprovedUserRoles(userModel);
                
                return Ok(new ItemResponse<UserModel>(ConstantMessage.Success, userModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("test-token")]
        public IActionResult TestToken()
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("No token provided");
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var roles = jwtToken.Claims
                .Where(c => c.Type == "role")
                .Select(c => c.Value)
                .ToList();

            return Ok(new
            {
                token = token,
                claims = jwtToken.Claims.Select(c => new { c.Type, c.Value }),
                roles = roles
            });
        }

        [HttpPost("change-password-first-time")]
        public async Task<IActionResult> ChangePasswordFirstTime([FromBody] ChangePasswordFirstTimeRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UsernameOrEmail) || string.IsNullOrEmpty(request.CurrentPassword) || string.IsNullOrEmpty(request.NewPassword))
                {
                    return BadRequest(new BaseResponse(false, "Vui lòng nhập đầy đủ thông tin"));
                }

                var authModel = new AuthModel
                {
                    UsernameOrEmail = request.UsernameOrEmail,
                    Password = request.CurrentPassword
                };

                var (result, message) = await _service.ChangePasswordFirstTime(authModel, request.NewPassword);
                
                return result switch
                {
                    null => BadRequest(new BaseResponse(false, message)),
                    not null => Ok(new ItemResponse<UserModel>(message, result))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse(false, ex.Message));
            }
        }

        [HttpPost("test-email")]
        public async Task<IActionResult> TestEmail([FromBody] TestEmailRequest request)
        {
            try
            {
                var emailService = HttpContext.RequestServices.GetRequiredService<IUserAccountEmailService>();
                
                var emailModel = new UserAccountEmailModel
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    FullName = request.FullName,
                    Phone = request.Phone,
                    Address = request.Address,
                    DOB = request.DOB,
                    Gender = request.Gender,
                    TemporaryPassword = request.TemporaryPassword,
                    CreatedDate = DateTime.Now,
                    LoginUrl = "https://smart-book-street-next-aso3.vercel.app/login",
                    BaseImgUrl = request.BaseImgUrl,
                    RequestedRoleName = request.RequestedRoleName
                };

                var result = await emailService.SendAccountCreatedEmailAsync(emailModel);
                
                return result 
                    ? Ok(new BaseResponse(true, "Email đã được gửi thành công"))
                    : BadRequest(new BaseResponse(false, "Không thể gửi email"));
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse(false, ex.Message));
            }
        }
    }
}
