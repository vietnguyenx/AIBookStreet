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
    public class BookCategoryService(IUnitOfWork repository, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseService<BookCategory>(mapper, repository, httpContextAccessor), IBookCategoryService
    {
        private readonly IUnitOfWork _repository = repository;
        public async Task<(BookCategory?, long)> AddABookCategory(BookCategoryModel model)
        {
            var existed = await _repository.BookCategoryRepository.GetByElement(model.BookId, model.CategoryId);
            if (existed != null && existed.Count > 0)
            {
                return (null, 1); //da ton tai
            }

            var bookCategory = _mapper.Map<BookCategory>(model);
            var setBookCategory = await SetBaseEntityToCreateFunc(bookCategory);
            var isSuccess = await _repository.BookCategoryRepository.Add(setBookCategory);
            if (isSuccess)
            {
                return (setBookCategory, 2); //thanh cong
            }
            return (null, 3);//fail
        }
        public async Task<(long, BookCategory?)> UpdateABookCategory(Guid id, BookCategoryModel model)
        {
            var existed = await _repository.BookCategoryRepository.GetByID(id);
            if (existed == null)
            {
                return (1, null); //khong ton tai
            }
            if (existed.IsDeleted)
            {
                return (3, null);
            }
            existed.BookId = model.BookId;
            existed.CategoryId = model.CategoryId;
            existed = await SetBaseEntityToUpdateFunc(existed);
            return await _repository.BookCategoryRepository.Update(existed) ? (2, existed) //update thanh cong
                                                                          : (3, null);       //update fail
        }
        public async Task<(long, BookCategory?)> DeleteABookCategory(Guid id)
        {
            var existed = await _repository.BookCategoryRepository.GetByID(id);
            if (existed == null)
            {
                return (1, null); //khong ton tai
            }
            existed = await SetBaseEntityToUpdateFunc(existed);
            return await _repository.BookCategoryRepository.Delete(existed) ? (2, existed) //delete thanh cong
                                                                          : (3, null);       //delete fail
        }
        public async Task<BookCategory?> GetABookCategoryById(Guid id)
        {
            return await _repository.BookCategoryRepository.GetByID(id);
        }
        public async Task<List<BookCategory>?> GetAllActiveBookCategories()
        {
            var bookCategories = await _repository.BookCategoryRepository.GetAll();

            return bookCategories.Count == 0 ? null : bookCategories;
        }
        public async Task<(List<BookCategory>?, long)> GetAllBookCategoriesPagination(string? key, Guid? bookID, Guid? categoryID, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var user = await GetUserInfo();
            var isAdmin = false;
            foreach (var userRole in user.UserRoles)
            {
                if (userRole.Role.RoleName == "Quản trị viên")
                {
                    isAdmin = true;
                }
            }
            var bookCategories = (user != null && isAdmin) ? await _repository.BookCategoryRepository.GetAllPaginationForAdmin(key, bookID, categoryID, pageNumber, pageSize, sortField, desc) 
                                                            : await _repository.BookCategoryRepository.GetAllPagination(key, bookID, categoryID, pageNumber, pageSize, sortField, desc);
            return bookCategories.Item1.Count() > 0 ? (bookCategories.Item1, bookCategories.Item2) : (null, 0);
        }
        public async Task<List<BookCategory>?> GetBookCategoryByElement(Guid? bookID, Guid? categoryID)
        {
            return await _repository.BookCategoryRepository.GetByElement(bookID, categoryID);
        }
    }
}
