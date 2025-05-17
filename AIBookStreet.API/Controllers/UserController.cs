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

        [HttpGet("by-email/{email}")]
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
                    return Ok(new LoginResponse<UserModel>(null, null, null, ConstantMessage.Fail));
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
                    return BadRequest(new BaseResponse(false, "User data is required"));
                }
                
                if (string.IsNullOrEmpty(userRequest.UserName) || string.IsNullOrEmpty(userRequest.Password))
                {
                    Console.WriteLine($"Registration failed: Username or password missing");
                    return BadRequest(new BaseResponse(false, "Username and password are required"));
                }

                // Create basic user model without using mapper
                Console.WriteLine("Creating a simple UserModel manually");
                var userModel = new UserModel
                {
                    UserName = userRequest.UserName,
                    Password = userRequest.Password,
                    Email = userRequest.Email,
                    FullName = userRequest.FullName,
                    DOB = userRequest.DOB,
                    Address = userRequest.Address,
                    Phone = userRequest.Phone,
                    Gender = userRequest.Gender
                };
                
                // Try simplified registration
                Console.WriteLine("Calling simplified registration method");
                UserModel registeredUser;
                try 
                {
                    registeredUser = await _service.RegisterSimple(userModel);
                }
                catch (Exception regEx)
                {
                    Console.WriteLine($"Registration service threw exception: {regEx.Message}");
                    if (regEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {regEx.InnerException.Message}");
                    }
                    return BadRequest(new BaseResponse(false, "Registration failed: " + regEx.Message));
                }

                if (registeredUser == null)
                {
                    Console.WriteLine("Simplified registration returned null");
                    return BadRequest(new BaseResponse(false, "User registration failed. The username or email may already be in use."));
                }

                // Return success with user data
                Console.WriteLine("Registration successful");
                return Ok(new ItemResponse<UserModel>(ConstantMessage.Success, registeredUser));
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"CRITICAL ERROR in registration controller: {ex.Message}");
                Console.WriteLine($"Exception type: {ex.GetType().FullName}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner exception type: {ex.InnerException.GetType().FullName}");
                }
                
                return BadRequest(new BaseResponse(false, $"An error occurred during registration: {ex.Message}"));
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
    }
}
