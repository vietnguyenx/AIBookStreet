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
        private DateTime countDown = DateTime.Now.AddDays(0.5);
        private static readonly Dictionary<string, (string Otp, DateTime Expiry)> OtpStore = new();

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration, IHttpContextAccessor _httpContextAccessor, IImageService imageService) : base(mapper, unitOfWork, _httpContextAccessor)
        {
            _repository = unitOfWork.UserRepository;
            _configuration = configuration;
            _imageService = imageService;
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

                var existingUser = await _userRepository.SearchWithoutPagination(new User { UserName = userModel.UserName });
                if (existingUser?.Any() == true)
                    return (null, ConstantMessage.User.UsernameExists);

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

                var result = await _userRepository.Add(newUser);
                if (!result)
                    return (null, ConstantMessage.Common.AddFail);

                return (_mapper.Map<UserModel>(newUser), ConstantMessage.Common.AddSuccess);
            }
            catch (Exception ex)
            {
                return (null, $"Error while adding user: {ex.Message}");
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

                var existingUser = await _userRepository.GetById(userModel.Id);
                if (existingUser == null)
                    return (null, ConstantMessage.Common.NotFoundForUpdate);

                if (!string.IsNullOrEmpty(userModel.UserName) && userModel.UserName != existingUser.UserName)
                {
                    var userWithSameUsername = await _userRepository.SearchWithoutPagination(new User { UserName = userModel.UserName });
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

                var result = await _userRepository.Update(updatedUser);
                if (!result)
                    return (null, ConstantMessage.Common.UpdateFail);

                return (_mapper.Map<UserModel>(updatedUser), ConstantMessage.Common.UpdateSuccess);
            }
            catch (Exception ex)
            {
                return (null, $"Error while updating user: {ex.Message}");
            }
        }

        public async Task<(UserModel?, string)> Delete(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return (null, ConstantMessage.EmptyId);

                var existingUser = await _userRepository.GetById(userId);
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

                var result = await _userRepository.Delete(existingUser);
                if (!result)
                    return (null, ConstantMessage.Common.DeleteFail);

                return (_mapper.Map<UserModel>(existingUser), ConstantMessage.Common.DeleteSuccess);
            }
            catch (Exception ex)
            {
                return (null, $"Error while deleting user: {ex.Message}");
            }
        }

        public async Task<UserModel> Login(AuthModel authModel)
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
                return null;
            }

            // check password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(authModel.Password, user.Password);
            if (!isPasswordValid)
            {
                return null;
            }

            UserModel userModel = _mapper.Map<UserModel>(user);

            return userModel;
        }

        public async Task<UserModel?> Register(UserModel userModel)
        {
            if (userModel.Password != null)
            {
                userModel.Password = BCrypt.Net.BCrypt.HashPassword(userModel.Password);
            }

            var (addedUser, message) = await Add(userModel);
            if (addedUser == null)
            {
                return null;
            }

            return await GetUserByEmailOrUsername(userModel);
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
    }

}
