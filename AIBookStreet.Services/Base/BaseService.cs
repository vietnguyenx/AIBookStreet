using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using AIBookStreet.Repositories.Repositories.Repositories.Repository;
using AIBookStreet.Repositories.Repositories.UnitOfWork.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Base
{
    public abstract class BaseService
    {

    }

    public abstract class BaseService<TEntity> : BaseService
        where TEntity : BaseEntity
    {
        protected readonly IMapper _mapper;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IUserRepository _userRepository;
        protected readonly IBaseRepository<TEntity> _baseRepository;

        protected BaseService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userRepository = _unitOfWork.UserRepository;
            _baseRepository = _unitOfWork.GetRepositoryByEntity<TEntity>();
        }

        protected BaseService(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor) : this(mapper, unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TEntity> SetBaseEntityToCreateFunc(TEntity entity)
        {

            var user = await GetUserInfo();
            if (user != null)
            {
                // đã đăng nhập
                entity.Id = Guid.NewGuid();
                entity.CreatedBy = user.Email;
                entity.CreatedDate = DateTime.Now;
                entity.LastUpdatedBy = user.Email;
                entity.LastUpdatedDate = entity.CreatedDate;
                entity.IsDeleted = false;
            }
            else
            {
                entity.Id = Guid.NewGuid();
                entity.CreatedDate = DateTime.Now;
                entity.IsDeleted = false;
            }

            return entity;
        }

        public async Task<TEntity> SetBaseEntityToUpdateFunc(TEntity entity)
        {

            var user = await GetUserInfo();
            if (user != null)
            {
                entity.LastUpdatedBy = user.Email;
                entity.LastUpdatedDate = DateTime.Now;
            }

            return entity;
        }

        public async Task<User> GetUserInfo()
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
                        var user = await _userRepository.FindUsernameOrEmail(new User
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

        public Task<long> GetTotalCount()
        {
            return _baseRepository.GetTotalCount();
        }

        private (string, string) GetUserEmailWithUserenameFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var emailClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "email"); // hoac bat ki claim nao chua email
            var usernameClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "sub"); // hoac bat ki claim nao chua email
            return (emailClaim?.Value, usernameClaim?.Value);
        }

    }


}
