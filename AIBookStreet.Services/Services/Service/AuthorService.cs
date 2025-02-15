using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using AIBookStreet.Repositories.Repositories.UnitOfWork.Interface;
using AIBookStreet.Services.Base;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Service
{
    public class AuthorService(IUnitOfWork repository, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseService<Author>(mapper, repository, httpContextAccessor), IAuthorService
    {
        private readonly IUnitOfWork _repository = repository;
        public async Task<Author?> AddAnAuthor(AuthorModel authorModel)
        {
            authorModel.DOB = authorModel.DOB?.ToLocalTime();
            var author = _mapper.Map<Author>(authorModel);
            var setAuthor = await SetBaseEntityToCreateFunc(author);
            var isSuccess = await _repository.AuthorRepository.Add(setAuthor);
            if (isSuccess)
            {
                return setAuthor;
            }
            return null;
        }
        public async Task<(long, Author?)> UpdateAnAuthor(Guid? authorID, AuthorModel authorModel)
        {
            var existAuthor = await _repository.AuthorRepository.GetByID(authorID);
            if (existAuthor == null)
            {
                return (1, null); //author khong ton tai
            }
            if (existAuthor.IsDeleted)
            {
                return (3, null);
            }
            existAuthor.AuthorName = authorModel.AuthorName;
            existAuthor.DOB = authorModel.DOB != null ? authorModel.DOB.Value.ToLocalTime() : existAuthor.DOB;
            existAuthor.Nationality = authorModel.Nationality ?? existAuthor.Nationality;
            existAuthor.Biography = authorModel.Biography ?? existAuthor.Biography;
            existAuthor = await SetBaseEntityToUpdateFunc(existAuthor);
            return await _repository.AuthorRepository.Update(existAuthor) ? (2, existAuthor) //update thanh cong
                                                                          : (3, null);       //update fail
        }
        public async Task<(long, Author?)> DeleteAnAuthor(Guid id)
        {
            var existAuthor = await _repository.AuthorRepository.GetByID(id);
            if (existAuthor == null)
            {
                return (1, null); //author khong ton tai
            }            
            existAuthor = await SetBaseEntityToUpdateFunc(existAuthor);

            return await _repository.AuthorRepository.Delete(existAuthor) ? (2, existAuthor) //delete thanh cong
                                                                          : (3, null);       //delete fail
        }
        public async Task<Author?> GetAnAuthorById(Guid id)
        {
            return await _repository.AuthorRepository.GetByID(id);
        }
        public async Task<List<Author>?> GetAllActiveAuthors(string? authorName)
        {
            var authors = await _repository.AuthorRepository.GetAll(authorName);

            return authors.Count == 0 ? null : authors;
        }
        public async Task<(List<Author>?, long)> GetAllAuthorsPagination(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var user = await GetUserInfo();
            var isAdmin = false;
            if (user != null) {
                foreach (var userRole in user.UserRoles)
                {
                    if (userRole.Role.RoleName == "Quản trị viên")
                    {
                        isAdmin = true;
                    }
                }
            }
            var authors = isAdmin ? await _repository.AuthorRepository.GetAllPaginationForAdmin(key, pageNumber, pageSize, sortField, desc) 
                                        : await _repository.AuthorRepository.GetAllPagination(key, pageNumber, pageSize, sortField, desc);
            return authors.Item1.Count > 0 ? (authors.Item1, authors.Item2) : (null, 0);
        }
    }
}
