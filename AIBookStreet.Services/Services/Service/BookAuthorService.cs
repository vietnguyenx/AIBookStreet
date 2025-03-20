using AIBookStreet.Repositories.Data.Entities;
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
    public class BookAuthorService(IUnitOfWork repository, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseService<BookAuthor>(mapper, repository, httpContextAccessor), IBookAuthorService
    {
        private readonly IUnitOfWork _repository = repository;
        public async Task<(BookAuthor?, long)> AddABookAuthor(BookAuthorModel model)
        {
            var existed = await _repository.BookAuthorRepository.GetByElement(model.BookId, model.AuthorId);
            if (existed != null && existed.Count > 0)
            {
                return (null, 1); //da ton tai
            }

            var bookAuthor = _mapper.Map<BookAuthor>(model);
            var setBookAuthor = await SetBaseEntityToCreateFunc(bookAuthor);
            var isSuccess = await _repository.BookAuthorRepository.Add(setBookAuthor);
            if (isSuccess)
            {
                return (setBookAuthor, 2); //thanh cong
            }
            return (null, 3);//fail
        }
        public async Task<(long, BookAuthor?)> UpdateABookAuthor(BookAuthorModel model)
        {
            var existed = await _repository.BookAuthorRepository.GetByElement(model.BookId, model.AuthorId);
            if (existed == null)
            {
                return (1, null); //khong ton tai
            }
            if (existed[0].IsDeleted)
            {
                return (3, null);
            }
            existed[0].BookId = model.BookId;
            existed[0].AuthorId = model.AuthorId;
            existed[0] = await SetBaseEntityToUpdateFunc(existed[0]);
            return await _repository.BookAuthorRepository.Update(existed[0]) ? (2, existed[0]) //update thanh cong
                                                                          : (3, null);       //update fail
        }
        public async Task<(long, BookAuthor?)> DeleteABookAuthor(BookAuthorModel model)
        {
            var existed = await _repository.BookAuthorRepository.GetByElement(model.BookId, model.AuthorId);
            if (existed == null)
            {
                return (1, null); //khong ton tai
            }
            existed[0] = await SetBaseEntityToUpdateFunc(existed[0]);
            return await _repository.BookAuthorRepository.Delete(existed[0]) ? (2, existed[0]) //delete thanh cong
                                                                          : (3, null);       //delete fail
        }
        public async Task<BookAuthor?> GetABookAuthorById(Guid id)
        {
            return await _repository.BookAuthorRepository.GetByID(id);
        }
        public async Task<List<BookAuthor>?> GetAllActiveBookAuthors()
        {
            var bookAuthors = await _repository.BookAuthorRepository.GetAll();

            return bookAuthors.Count == 0 ? null : bookAuthors;
        }
        public async Task<(List<BookAuthor>?, long)> GetAllBookAuthorsPagination(string? key, Guid? bookID, Guid? authorID, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var user = await GetUserInfo();
            var isAdmin = false;
            if (user != null)
            {
                foreach (var userRole in user.UserRoles)
                {
                    if (userRole.Role.RoleName == "Quản trị viên")
                    {
                        isAdmin = true;
                    }
                }
            }
            var bookAuthors = isAdmin ? await _repository.BookAuthorRepository.GetAllPaginationForAdmin(key, bookID, authorID, pageNumber, pageSize, sortField, desc) : 
                                                            await _repository.BookAuthorRepository.GetAllPagination(key, bookID, authorID, pageNumber, pageSize, sortField, desc);
            return bookAuthors.Item1.Count > 0 ? (bookAuthors.Item1, bookAuthors.Item2) : (null, 0);
        }
        public async Task<List<BookAuthor>?> GetBookAuthorByElement(Guid? bookID, Guid? authorID)
        {
            return await _repository.BookAuthorRepository.GetByElement(bookID, authorID);
        }
    }
}
