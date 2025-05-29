using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using AIBookStreet.Repositories.Repositories.Repositories.Repository;
using AIBookStreet.Repositories.Repositories.UnitOfWork.Interface;
using AIBookStreet.Services.Base;
using AIBookStreet.Services.Common;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Service
{
    public class UserService : BaseService<User>, IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IImageService _imageService;
        private readonly IConfiguration _configuration;
        private readonly IUserAccountEmailService _userAccountEmailService;
        private DateTime countDown = DateTime.Now.AddDays(7);
        private static readonly Dictionary<string, (string Otp, DateTime Expiry)> OtpStore = new();

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration, IHttpContextAccessor _httpContextAccessor, IImageService imageService, IUserAccountEmailService userAccountEmailService) : base(mapper, unitOfWork, _httpContextAccessor)
        {
            _repository = unitOfWork.UserRepository;
            _configuration = configuration;
            _imageService = imageService;
            _userAccountEmailService = userAccountEmailService;
        }

        public async Task<List<UserModel>> GetAll()
        {
            var users = await _repository.GetAll();

            if (!users.Any())
            {
                return null;
            }

            return _mapper.Map<List<UserModel>>(users);
        }

        public async Task<List<UserModel>?> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var users = await _repository.GetAllPagination(pageNumber, pageSize, sortField, sortOrder);

            if (!users.Any())
            {
                return null;
            }

            return _mapper.Map<List<UserModel>>(users);
        }

        public async Task<UserModel?> GetById(Guid id)
        {
            var user = await _repository.GetById(id);

            if (user == null)
            {
                return null;
            }

            return _mapper.Map<UserModel>(user);
        }

        public async Task<(List<UserModel>?, long)> SearchPagination(UserModel userModel, int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var user = _mapper.Map<User>(userModel);
            var usersWithTotalOrigin = await _repository.SearchPagination(user, pageNumber, pageSize, sortField, sortOrder);

            if (!usersWithTotalOrigin.Item1.Any())
            {
                return (null, usersWithTotalOrigin.Item2);
            }
            var userModels = _mapper.Map<List<UserModel>>(usersWithTotalOrigin.Item1);

            return (userModels, usersWithTotalOrigin.Item2);
        }

        public async Task<List<UserModel>?> SearchWithoutPagination(UserModel userModel)
        {
            var user = _mapper.Map<User>(userModel);
            var users = await _repository.SearchWithoutPagination(user);

            if (!users.Any())
            {
                return null;
            }

            return _mapper.Map<List<UserModel>>(users);
        }

        public async Task<(UserModel?, string)> Add(UserModel userModel)
        {
            try
            {
                if (userModel == null)
                    return (null, ConstantMessage.Common.EmptyInfo);

                if (string.IsNullOrEmpty(userModel.UserName))
                    return (null, ConstantMessage.User.EmptyUsername);

                if (string.IsNullOrEmpty(userModel.Password))
                    return (null, ConstantMessage.User.EmptyPassword);

                // Validate username format
                if (!ValidationRules.IsValidUsername(userModel.UserName))
                    return (null, "Tên đăng nhập không hợp lệ. Tên đăng nhập chỉ được chứa chữ cái, số và dấu gạch dưới, độ dài từ 3-50 ký tự.");

                // Validate email format
                if (!string.IsNullOrEmpty(userModel.Email) && !ValidationRules.IsValidEmail(userModel.Email))
                    return (null, ConstantMessage.Validation.InvalidEmailFormat);

                // Validate password strength
                if (!ValidationRules.IsValidPassword(userModel.Password))
                    return (null, ConstantMessage.Validation.PasswordTooWeak);

                // Validate full name if provided
                if (!string.IsNullOrEmpty(userModel.FullName) && !ValidationRules.IsValidFullName(userModel.FullName))
                    return (null, "Họ tên không hợp lệ. Họ tên chỉ được chứa chữ cái, dấu cách và dấu gạch ngang, độ dài từ 2-100 ký tự.");

                // Validate phone if provided
                if (!string.IsNullOrEmpty(userModel.Phone) && !ValidationRules.IsValidPhone(userModel.Phone))
                    return (null, ConstantMessage.Validation.InvalidPhoneFormat);

                var existingUser = await _repository.SearchWithoutPagination(new User { UserName = userModel.UserName });
                if (existingUser?.Any() == true)
                    return (null, ConstantMessage.User.UsernameExists);
                    
                // Check if email is already in use
                if (!string.IsNullOrEmpty(userModel.Email))
                {
                    var existingUserByEmail = await _repository.GetUserByEmail(new User { Email = userModel.Email });
                    if (existingUserByEmail != null)
                        return (null, "Email đã được sử dụng, vui lòng sử dụng email khác");
                }

                // Lưu mật khẩu gốc để gửi email
                var originalPassword = userModel.Password;

                var mappedUser = _mapper.Map<User>(userModel);
                var newUser = await SetBaseEntityToCreateFunc(mappedUser);

                if (userModel.MainImageFile != null)
                {
                    if (userModel.MainImageFile.Length > 10 * 1024 * 1024)
                        return (null, ConstantMessage.Image.MainImageSizeExceeded);

                    if (!userModel.MainImageFile.ContentType.StartsWith("image/"))
                        return (null, ConstantMessage.Image.InvalidMainImageFormat);

                    var mainImageModel = new FileModel
                    {
                        File = userModel.MainImageFile,
                        Type = "user_main",
                        AltText = userModel.UserName ?? userModel.MainImageFile.FileName,
                        EntityId = newUser.Id
                    };

                    var mainImages = await _imageService.AddImages(new List<FileModel> { mainImageModel });
                    if (mainImages == null || !mainImages.Any())
                        return (null, ConstantMessage.Image.MainImageUploadFailed);

                    newUser.BaseImgUrl = mainImages.First().Url;
                }

                if (userModel.AdditionalImageFiles?.Any() == true)
                {
                    foreach (var file in userModel.AdditionalImageFiles)
                    {
                        if (file.Length > 10 * 1024 * 1024)
                            return (null, ConstantMessage.Image.SubImageSizeExceeded);

                        if (!file.ContentType.StartsWith("image/"))
                            return (null, ConstantMessage.Image.InvalidSubImageFormat);
                    }

                    var additionalImageModels = userModel.AdditionalImageFiles.Select(file => new FileModel
                    {
                        File = file,
                        Type = "user_additional",
                        AltText = userModel.UserName ?? file.FileName,
                        EntityId = newUser.Id
                    }).ToList();

                    var additionalImages = await _imageService.AddImages(additionalImageModels);
                    if (additionalImages == null)
                        return (null, ConstantMessage.Image.SubImageUploadFailed);
                }

                var result = await _repository.Add(newUser);
                if (!result)
                    return (null, ConstantMessage.Common.AddFail);

                if (userModel.RequestedRoleId.HasValue && userModel.RequestedRoleId != Guid.Empty)
                {
                    var userRoleModel = new UserRoleModel
                    {
                        UserId = newUser.Id,
                        RoleId = userModel.RequestedRoleId.Value,
                        AssignedAt = DateTime.Now,
                        IsApproved = false 
                    };
                    
                    var userRoleService = new UserRoleService(_unitOfWork, _mapper, _httpContextAccessor, _userAccountEmailService, _configuration);
                    var addRoleResult = await userRoleService.Add(userRoleModel);
                    if (!addRoleResult)
                    {
                        Console.WriteLine("Failed to create UserRole");
                    }
                }

                if (!string.IsNullOrEmpty(newUser.Email))
                {
                    try
                    {
                        string requestedRoleName = null;
                        if (userModel.RequestedRoleId.HasValue && userModel.RequestedRoleId != Guid.Empty)
                        {
                            var roleService = new RoleService(_unitOfWork, _mapper, _httpContextAccessor);
                            var role = await roleService.GetById(userModel.RequestedRoleId.Value);
                            requestedRoleName = role?.RoleName;
                        }

                        var emailModel = new UserAccountEmailModel
                        {
                            UserName = newUser.UserName,
                            Email = newUser.Email,
                            FullName = newUser.FullName ?? newUser.UserName,
                            Phone = newUser.Phone,
                            Address = newUser.Address,
                            DOB = newUser.DOB,
                            Gender = newUser.Gender,
                            TemporaryPassword = originalPassword,
                            CreatedDate = newUser.CreatedDate,
                            LoginUrl = _configuration["AppSettings:LoginUrl"] ?? "https://smart-book-street-next-aso3.vercel.app/login",
                            BaseImgUrl = newUser.BaseImgUrl,
                            RequestedRoleId = userModel.RequestedRoleId,
                            RequestedRoleName = requestedRoleName
                        };

                        var emailSent = await _userAccountEmailService.SendAccountCreatedEmailAsync(emailModel);
                        if (!emailSent)
                        {
                            Console.WriteLine($"Không thể gửi email thông báo tài khoản cho {newUser.Email}");
                        }
                    }
                    catch (Exception emailEx)
                    {
                        Console.WriteLine($"Lỗi khi gửi email thông báo tài khoản: {emailEx.Message}");
                    }
                }

                return (_mapper.Map<UserModel>(newUser), ConstantMessage.Common.AddSuccess);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi thêm người dùng: {ex.Message}");
            }
        }

        public async Task<(UserModel?, string)> Update(UserModel userModel)
        {
            try
            {
                if (userModel == null)
                    return (null, ConstantMessage.Common.EmptyInfo);

                if (userModel.Id == Guid.Empty)
                    return (null, ConstantMessage.EmptyId);

                var existingUser = await _repository.GetById(userModel.Id);
                if (existingUser == null)
                    return (null, ConstantMessage.Common.NotFoundForUpdate);

                if (!string.IsNullOrEmpty(userModel.UserName) && userModel.UserName != existingUser.UserName)
                {
                    var userWithSameUsername = await _repository.SearchWithoutPagination(new User { UserName = userModel.UserName });
                    if (userWithSameUsername?.Any() == true)
                        return (null, ConstantMessage.User.UsernameExists);
                }

                if (string.IsNullOrEmpty(userModel.UserName))
                    userModel.UserName = existingUser.UserName;

                _mapper.Map(userModel, existingUser);
                var updatedUser = await SetBaseEntityToUpdateFunc(existingUser);

                if (userModel.MainImageFile != null)
                {
                    if (userModel.MainImageFile.Length > 10 * 1024 * 1024)
                        return (null, ConstantMessage.Image.MainImageSizeExceeded);

                    if (!userModel.MainImageFile.ContentType.StartsWith("image/"))
                        return (null, ConstantMessage.Image.InvalidMainImageFormat);

                    var existingMainImages = await _imageService.GetImagesByTypeAndEntityID("user_main", updatedUser.Id);
                    if (existingMainImages?.Any() == true)
                    {
                        var mainImageModel = new FileModel
                        {
                            File = userModel.MainImageFile,
                            Type = "user_main",
                            AltText = userModel.UserName ?? userModel.MainImageFile.FileName,
                            EntityId = updatedUser.Id
                        };

                        var updateResult = await _imageService.UpdateAnImage(existingMainImages.First().Id, mainImageModel);
                        if (updateResult.Item1 != 2)
                            return (null, ConstantMessage.Image.MainImageUploadFailed);

                        updatedUser.BaseImgUrl = updateResult.Item2.Url;
                    }
                    else
                    {
                        var mainImageModel = new FileModel
                        {
                            File = userModel.MainImageFile,
                            Type = "user_main",
                            AltText = userModel.UserName ?? userModel.MainImageFile.FileName,
                            EntityId = updatedUser.Id
                        };

                        var mainImages = await _imageService.AddImages(new List<FileModel> { mainImageModel });
                        if (mainImages == null)
                            return (null, ConstantMessage.Image.MainImageUploadFailed);

                        updatedUser.BaseImgUrl = mainImages.First().Url;
                    }
                }

                if (userModel.AdditionalImageFiles?.Any() == true)
                {
                    foreach (var file in userModel.AdditionalImageFiles)
                    {
                        if (file.Length > 10 * 1024 * 1024)
                            return (null, ConstantMessage.Image.SubImageSizeExceeded);

                        if (!file.ContentType.StartsWith("image/"))
                            return (null, ConstantMessage.Image.InvalidSubImageFormat);
                    }

                    var existingAdditionalImages = await _imageService.GetImagesByTypeAndEntityID("user_additional", updatedUser.Id);
                    if (existingAdditionalImages?.Any() == true)
                    {
                        foreach (var image in existingAdditionalImages)
                        {
                            var deleteResult = await _imageService.DeleteAnImage(image.Id);
                            if (deleteResult.Item1 != 2)
                                return (null, ConstantMessage.Image.SubImageUploadFailed);
                        }
                    }

                    var additionalImageModels = userModel.AdditionalImageFiles.Select(file => new FileModel
                    {
                        File = file,
                        Type = "user_additional",
                        AltText = userModel.UserName ?? file.FileName,
                        EntityId = updatedUser.Id
                    }).ToList();

                    var additionalImages = await _imageService.AddImages(additionalImageModels);
                    if (additionalImages == null)
                        return (null, ConstantMessage.Image.SubImageUploadFailed);
                }
                if (!string.IsNullOrEmpty(userModel.Password))
                    updatedUser.Password = BCrypt.Net.BCrypt.HashPassword(userModel.Password);
                var result = await _repository.Update(updatedUser);
                if (!result)
                    return (null, ConstantMessage.Common.UpdateFail);

                return (_mapper.Map<UserModel>(updatedUser), ConstantMessage.Common.UpdateSuccess);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi cập nhật người dùng: {ex.Message}");
            }
        }

        public async Task<(UserModel?, string)> Delete(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return (null, ConstantMessage.EmptyId);

                var existingUser = await _repository.GetById(userId);
                if (existingUser == null)
                    return (null, ConstantMessage.Common.NotFoundForDelete);

                var existingMainImages = await _imageService.GetImagesByTypeAndEntityID("user_main", userId);
                var existingAdditionalImages = await _imageService.GetImagesByTypeAndEntityID("user_additional", userId);

                if (existingMainImages != null)
                {
                    foreach (var image in existingMainImages)
                    {
                        var deleteResult = await _imageService.DeleteAnImage(image.Id);
                        if (deleteResult.Item1 != 2)
                            return (null, ConstantMessage.Image.MainImageUploadFailed);
                    }
                }

                if (existingAdditionalImages != null)
                {
                    foreach (var image in existingAdditionalImages)
                    {
                        var deleteResult = await _imageService.DeleteAnImage(image.Id);
                        if (deleteResult.Item1 != 2)
                            return (null, ConstantMessage.Image.SubImageUploadFailed);
                    }
                }

                var result = await _repository.Delete(existingUser);
                if (!result)
                    return (null, ConstantMessage.Common.DeleteFail);

                return (_mapper.Map<UserModel>(existingUser), ConstantMessage.Common.DeleteSuccess);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi xóa người dùng: {ex.Message}");
            }
        }

        public async Task<(UserModel, bool, string)> Login(AuthModel authModel)
        {
            User userHasUsernameOrEmail = new User
            {
                Email = authModel.UsernameOrEmail,
                UserName = authModel.UsernameOrEmail,
                Password = authModel.Password,
            };
            // check username or email
            User user = await _repository.FindUsernameOrEmail(userHasUsernameOrEmail);

            if (user == null)
            {
                return (null, false, null);
            }

            // Check if password is hashed
            bool passwordNeedsChange = false;
            try
            {
                BCrypt.Net.BCrypt.Verify("dummy", user.Password);
            }
            catch
            {
                // Password is not hashed, user needs to change password
                passwordNeedsChange = true;
                return (null, true, "Bạn đang đăng nhập lần đầu bằng tài khoản do Admin cung cấp. Vui lòng đổi mật khẩu trước khi tiếp tục.");
            }

            // check password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(authModel.Password, user.Password);
            if (!isPasswordValid)
            {
                throw new Exception("Mật khẩu không chính xác. Vui lòng thử lại.");
            }

            // Load user with roles
            user = await _repository.GetById(user.Id);
            UserModel userModel = _mapper.Map<UserModel>(user);

            // Lọc bỏ những userRole chưa được phê duyệt
            if (userModel.UserRoles != null && userModel.UserRoles.Any())
            {
                userModel.UserRoles = userModel.UserRoles.Where(ur => ur.IsApproved).ToList();
            }

            return (userModel, false, null);
        }

        public async Task<(UserModel?, string)> Register(UserModel userModel)
        {
            try
            {
                Console.WriteLine("Bắt đầu đăng ký người dùng cho: " + userModel.UserName);
                
                // Basic validation
                if (userModel == null)
                {
                    Console.WriteLine("Registration failed: User model is null");
                    return (null, "Thông tin người dùng không được để trống");
                }

                if (string.IsNullOrEmpty(userModel.UserName) || string.IsNullOrEmpty(userModel.Password))
                {
                    Console.WriteLine("Registration failed: Username or password is empty");
                    return (null, "Tên đăng nhập và mật khẩu không được để trống");
                }

                // Validate username format
                if (!ValidationRules.IsValidUsername(userModel.UserName))
                {
                    Console.WriteLine("Registration failed: Invalid username format");
                    return (null, "Tên đăng nhập không hợp lệ. Tên đăng nhập chỉ được chứa chữ cái, số và dấu gạch dưới, độ dài từ 3-50 ký tự.");
                }

                // Validate email format
                if (!string.IsNullOrEmpty(userModel.Email) && !ValidationRules.IsValidEmail(userModel.Email))
                {
                    Console.WriteLine("Registration failed: Invalid email format");
                    return (null, ConstantMessage.Validation.InvalidEmailFormat);
                }

                // Validate password strength
                if (!ValidationRules.IsValidPassword(userModel.Password))
                {
                    Console.WriteLine("Registration failed: Password does not meet requirements");
                    return (null, ConstantMessage.Validation.PasswordTooWeak);
                }

                // Check if user or email already exists
                if (!string.IsNullOrEmpty(userModel.Email))
                {
                    Console.WriteLine("Checking for existing email: " + userModel.Email);
                    var existingUserByEmail = await _repository.GetUserByEmail(new User { Email = userModel.Email });
                    if (existingUserByEmail != null)
                    {
                        Console.WriteLine("Registration failed: Email already exists");
                        return (null, "Email đã được sử dụng, vui lòng sử dụng email khác");
                    }
                }
                else
                {
                    Console.WriteLine("Registration failed: Email is required");
                    return (null, "Email là bắt buộc");
                }

                Console.WriteLine("Checking for existing username: " + userModel.UserName);
                var existingUserByUsername = await _repository.SearchWithoutPagination(new User { UserName = userModel.UserName });
                if (existingUserByUsername?.Any() == true)
                {
                    Console.WriteLine("Registration failed: Username already exists");
                    return (null, ConstantMessage.User.UsernameExists);
                }

                // Set defaults
                userModel.FullName ??= userModel.UserName;
                Console.WriteLine("Set fullname: " + userModel.FullName);
                
                // Clear ID to ensure new one is generated
                userModel.Id = Guid.Empty;
                
                try
                {
                    // Hash password - add explicit error handling
                    Console.WriteLine("Hashing password");
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userModel.Password);
                    userModel.Password = hashedPassword;
                }
                catch (Exception hashEx)
                {
                    Console.WriteLine("Password hashing failed: " + hashEx.Message);
                    return (null, "Lỗi khi mã hóa mật khẩu: " + hashEx.Message);
                }

                try
                {
                    // Create user entity and set required properties
                    Console.WriteLine("Mapping to User entity");
                    var user = _mapper.Map<User>(userModel);
                    
                    // Generate new ID
                    user.Id = Guid.NewGuid();
                    Console.WriteLine("Generated new user ID: " + user.Id);
                    
                    user.CreatedDate = DateTime.Now;
                    user.LastUpdatedDate = DateTime.Now;
                    user.IsDeleted = false;
                    
                    // Add to database directly
                    Console.WriteLine("Adding user to database");
                    var addResult = await _repository.Add(user);
                    if (!addResult)
                    {
                        Console.WriteLine("Database add operation returned false");
                        return (null, "Không thể thêm người dùng vào cơ sở dữ liệu");
                    }
                    
                    // Retrieve user with roles to return
                    Console.WriteLine("Retrieving created user with ID: " + user.Id);
                    var createdUser = await _repository.GetById(user.Id);
                    if (createdUser == null)
                    {
                        Console.WriteLine("Failed to retrieve created user");
                        return (null, "Không thể lấy thông tin người dùng sau khi tạo");
                    }
                    
                    Console.WriteLine("User registration successful for: " + user.UserName);
                    
                    // Xử lý tạo UserRole nếu có RequestedRoleId
                    if (userModel.RequestedRoleId.HasValue && userModel.RequestedRoleId != Guid.Empty)
                    {
                        Console.WriteLine("Creating UserRole for role ID: " + userModel.RequestedRoleId);
                        var userRoleModel = new UserRoleModel
                        {
                            UserId = user.Id,
                            RoleId = userModel.RequestedRoleId.Value,
                            AssignedAt = DateTime.Now,
                            IsApproved = false // Mặc định là chưa được phê duyệt
                        };
                        
                        var userRoleService = new UserRoleService(_unitOfWork, _mapper, _httpContextAccessor, _userAccountEmailService, _configuration);
                        var addRoleResult = await userRoleService.Add(userRoleModel);
                        if (!addRoleResult)
                        {
                            Console.WriteLine("Failed to create UserRole");
                        }
                    }
                    
                    return (_mapper.Map<UserModel>(createdUser), "Đăng ký thành công");
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine("Database operation failed: " + dbEx.Message);
                    Console.WriteLine("Stack trace: " + dbEx.StackTrace);
                    return (null, "Lỗi khi thao tác với cơ sở dữ liệu: " + dbEx.Message);
                }
            }
            catch (Exception ex)
            {
                // Log the exception with much more detail
                Console.WriteLine("CRITICAL ERROR in user registration: " + ex.Message);
                Console.WriteLine("Exception type: " + ex.GetType().FullName);
                Console.WriteLine("Stack trace: " + ex.StackTrace);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner exception: " + ex.InnerException.Message);
                    Console.WriteLine("Inner exception type: " + ex.InnerException.GetType().FullName);
                }
                return (null, "Lỗi không xác định khi đăng ký: " + ex.Message);
            }
        }

        public JwtSecurityToken CreateToken(UserModel userModel)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userModel.UserName),
            };
            // Conditional addition of claim based on function result
            if (!string.IsNullOrEmpty(userModel.Email))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, userModel.Email));
            }

            if (userModel.UserRoles != null && userModel.UserRoles.Any())
            {
                foreach (var userRole in userModel.UserRoles)
                {
                    // Chỉ thêm role đã được phê duyệt
                    if (userRole.Role != null && userRole.IsApproved)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, userRole.Role.RoleName));
                    }
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("Appsettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: creds,
                expires: countDown
                );

            return token;
        }

        public async Task<UserModel?> GetUserByEmailOrUsername(UserModel userModel)
        {
            var user = await _repository.FindUsernameOrEmail(_mapper.Map<User>(userModel));
            return _mapper.Map<UserModel>(user);
        }

        public async Task<UserModel?> GetUserByEmail(UserModel userModel)
        {
            var user = await _repository.GetUserByEmail(_mapper.Map<User>(userModel));
            return _mapper.Map<UserModel>(user);
        }

        public async Task<User?> GetUserInfo()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Request.Headers.ContainsKey("Authorization"))
            {
                var token = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (token == "null")
                {
                    return null;
                }
                if (!string.IsNullOrEmpty(token))
                {
                    var userFromToken = GetUserEmailWithUserenameFromToken(token);
                    if (userFromToken.Item1 != null && userFromToken.Item2 != null)
                    {
                        var user = await _repository.FindUsernameOrEmail(new User
                        {
                            Email = userFromToken.Item1,
                            UserName = userFromToken.Item2
                        });

                        if (user != null)
                        {
                            return user;
                        }
                    }
                }
            }

            return null;
        }

        private (string, string) GetUserEmailWithUserenameFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var emailClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "email");
            var usernameClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "sub");
            return (emailClaim?.Value, usernameClaim?.Value);
        }

        //public async Task<UserModel?> ProcessGoogleLoginAsync(ClaimsPrincipal claimsPrincipal)
        //{
        //    try
        //    {
        //        var email = claimsPrincipal.FindFirstValue(ClaimTypes.Email);
        //        var name = claimsPrincipal.FindFirstValue(ClaimTypes.Name);
        //        var nameid = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

        //        if (string.IsNullOrEmpty(email))
        //        {
        //            return null;
        //        }

        //        var existingUser = await _repository.GetUserByEmail(new User { Email = email });

        //        if (existingUser == null)
        //        {
        //            var username = email.Split('@')[0] + "_google";
                    
        //            var randomPassword = Guid.NewGuid().ToString();
        //            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(randomPassword);
                    
        //            var newUser = new UserModel
        //            {
        //                Email = email,
        //                UserName = username,
        //                FullName = name,
        //                Password = hashedPassword,
        //            };

        //            var registeredUser = await Register(newUser);
        //            return registeredUser;
        //        }

        //        return _mapper.Map<UserModel>(existingUser);
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}

        public async Task<UserModel?> RegisterSimple(UserModel userModel)
        {
            try
            {
                Console.WriteLine("Starting simplified registration for: " + userModel.UserName);
                
                // Validate basic requirements
                if (string.IsNullOrEmpty(userModel.UserName) || string.IsNullOrEmpty(userModel.Password))
                {
                    Console.WriteLine("Simplified registration failed: Missing required fields");
                    return null;
                }
                
                // Check for existing username - using direct string comparison
                Console.WriteLine("Checking for existing username: " + userModel.UserName);
                var existingUsers = await _repository.GetAll();
                var userWithSameUsername = existingUsers.FirstOrDefault(u => 
                    string.Equals(u.UserName, userModel.UserName, StringComparison.OrdinalIgnoreCase));
                    
                if (userWithSameUsername != null)
                {
                    Console.WriteLine("Username already exists");
                    return null;
                }
                
                // Check for existing email if provided
                if (!string.IsNullOrEmpty(userModel.Email))
                {
                    Console.WriteLine("Checking for existing email: " + userModel.Email);
                    // Sử dụng phương thức chuyên biệt để kiểm tra email
                    var existingUserByEmail = await _repository.GetUserByEmail(new User { Email = userModel.Email });
                    if (existingUserByEmail != null) 
                    {
                        Console.WriteLine("Email already exists");
                        return null;
                    }
                }
                
                // Create user entity directly without using mapper
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = userModel.UserName,
                    Password = BCrypt.Net.BCrypt.HashPassword(userModel.Password),
                    Email = userModel.Email,
                    FullName = !string.IsNullOrEmpty(userModel.FullName) ? userModel.FullName : userModel.UserName,
                    DOB = userModel.DOB,
                    Address = userModel.Address,
                    Phone = userModel.Phone,
                    Gender = userModel.Gender,
                    
                    // Set base entity properties
                    CreatedDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    IsDeleted = false
                };
                
                Console.WriteLine($"Created user entity with ID: {user.Id}");
                
                // Add user to database
                try
                {
                    Console.WriteLine("Adding user to database directly");
                    var result = await _repository.Add(user);
                    
                    if (!result)
                    {
                        Console.WriteLine("Database add operation failed");
                        return null;
                    }
                    
                    // Get the created user
                    Console.WriteLine("Retrieving created user");
                    var createdUser = await _repository.GetById(user.Id);
                    if (createdUser == null)
                    {
                        Console.WriteLine("Failed to retrieve created user");
                        return null;
                    }
                    
                    // Map back to model
                    var userModelResult = new UserModel
                    {
                        Id = createdUser.Id,
                        UserName = createdUser.UserName,
                        Email = createdUser.Email,
                        FullName = createdUser.FullName,
                        DOB = createdUser.DOB,
                        Address = createdUser.Address,
                        Phone = createdUser.Phone,
                        Gender = createdUser.Gender,
                        CreatedDate = createdUser.CreatedDate,
                        LastUpdatedDate = createdUser.LastUpdatedDate,
                        IsDeleted = createdUser.IsDeleted
                    };
                    
                    Console.WriteLine("Simplified registration successful");
                    return userModelResult;
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"Database operation failed: {dbEx.Message}");
                    Console.WriteLine($"Stack trace: {dbEx.StackTrace}");
                    if (dbEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {dbEx.InnerException.Message}");
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Simplified registration exception: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return null;
            }
        }

        public async Task<bool> IsPasswordHashed(string usernameOrEmail)
        {
            try
            {
                var user = await _repository.FindUsernameOrEmail(new User 
                { 
                    UserName = usernameOrEmail,
                    Email = usernameOrEmail 
                });

                if (user == null)
                    return false;

                try
                {
                    BCrypt.Net.BCrypt.Verify("dummy", user.Password);
                    return true;
                }
                catch
                {
                    return false; 
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<(UserModel?, string)> ChangePasswordFirstTime(AuthModel authModel, string newPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(authModel.UsernameOrEmail) || string.IsNullOrEmpty(authModel.Password))
                    return (null, "Vui lòng nhập đầy đủ thông tin đăng nhập");

                if (string.IsNullOrEmpty(newPassword))
                    return (null, "Vui lòng nhập mật khẩu mới");

                // Validate new password strength
                if (!ValidationRules.IsValidPassword(newPassword))
                    return (null, ConstantMessage.Validation.PasswordTooWeak);

                // Find user
                var user = await _repository.FindUsernameOrEmail(new User 
                { 
                    UserName = authModel.UsernameOrEmail,
                    Email = authModel.UsernameOrEmail 
                });

                if (user == null)
                    return (null, "Không tìm thấy tài khoản");

                // Verify current password (not hashed)
                if (user.Password != authModel.Password)
                    return (null, "Mật khẩu hiện tại không chính xác");

                // Check if password is already hashed
                try
                {
                    BCrypt.Net.BCrypt.Verify("dummy", user.Password);
                    return (null, "Mật khẩu đã được thay đổi trước đó");
                }
                catch
                {
                    // Password is not hashed, proceed with change
                }

                // Hash and update new password
                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.LastUpdatedDate = DateTime.Now;

                var result = await _repository.Update(user);
                if (!result)
                    return (null, "Không thể cập nhật mật khẩu");

                return (_mapper.Map<UserModel>(user), "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.");
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi đổi mật khẩu: {ex.Message}");
            }
        }
    }
}
