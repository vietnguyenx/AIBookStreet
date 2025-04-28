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
    public class CategoryService(IUnitOfWork repository, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseService<Category>(mapper, repository, httpContextAccessor), ICategoryService
    {
        private readonly IUnitOfWork _repository = repository;
        public async Task<Category?> AddACategory(CategoryModel model)
        {
            var category = _mapper.Map<Category>(model);
            var setCategory = await SetBaseEntityToCreateFunc(category);
            var isSuccess = await _repository.CategoryRepository.Add(setCategory);
            if (isSuccess)
            {
                return setCategory;
            }
            return null;
        }
        public async Task<(long, Category?)> UpdateACategory(Guid? id, CategoryModel model)
        {
            var existed = await _repository.CategoryRepository.GetByID(id);
            if (existed == null)
            {
                return (1, null); //khong ton tai
            }
            if (existed.IsDeleted)
            {
                return (3, null);
            }
            existed.CategoryName = model.CategoryName;
            existed.Description = model.Description ?? existed.Description;
            existed = await SetBaseEntityToUpdateFunc(existed);
            return await _repository.CategoryRepository.Update(existed) ? (2, existed) //update thanh cong
                                                                          : (3, null);       //update fail
        }
        public async Task<(long, Category?)> DeleteACategory(Guid id)
        {
            var existed = await _repository.CategoryRepository.GetByID(id);
            if (existed == null)
            {
                return (1, null); //khong ton tai
            }
            existed = await SetBaseEntityToUpdateFunc(existed);

            //var bookCategories = await _repository.BookAuthorRepository.GetByElement(null, id);
            //if (bookCategories != null)
            //{
            //    foreach (var bookCategory in bookCategories)
            //    {
            //        bookCategory.LastUpdatedBy = existed.LastUpdatedBy;
            //        bookCategory.LastUpdatedDate = DateTime.Now;
            //        bookCategory.IsDeleted = true;
            //    }
            //}

            return await _repository.CategoryRepository.Delete(existed) ? (2, existed) //delete thanh cong
                                                                          : (3, null);       //delete fail
        }
        public async Task<Category?> GetACategoryById(Guid id)
        {
            return await _repository.CategoryRepository.GetByID(id);
        }
        public async Task<List<Category>?> GetAllActiveCategories(string? name, Guid? authorId)
        {
            var bookCategories = new List<BookCategory>();
            var categoryIds = new List<Guid>();
            if (authorId != null)
            {
                var bookAuthors = await _repository.BookAuthorRepository.GetByElement(null, authorId);


                if (bookAuthors != null)
                {
                    foreach (var bookAuthor in bookAuthors)
                    {
                        var bcs = await _repository.BookCategoryRepository.GetByElement(bookAuthor.BookId, null);
                        if (bcs != null)
                        {
                            foreach (var bc in bcs)
                            {
                                bookCategories.Add(bc);
                            }
                        }
                    }
                }

                if (bookCategories != null)
                {
                    foreach (var bookCategory in bookCategories)
                    {
                        categoryIds.Add(bookCategory.CategoryId);
                    }
                }
            }
            var categories = await _repository.CategoryRepository.GetAll(name, categoryIds);

            return categories.Count == 0 ? null : categories;
        }
        public async Task<(List<Category>?, long)> GetAllCategoriesPagination(string? key, Guid? authorId, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var user = await GetUserInfo();
            var isAdmin = false;
            if (user != null)
            {
                foreach (var userRole in user.UserRoles)
                {
                    if (userRole.Role.RoleName == "Admin")
                    {
                        isAdmin = true;
                    }
                }
            }
            var bookCategories = new List<BookCategory>();
            var categoryIds = new List<Guid>();
            if (authorId != null)
            {
                var bookAuthors = await _repository.BookAuthorRepository.GetByElement(null, authorId);


                if (bookAuthors != null)
                {
                    foreach (var bookAuthor in bookAuthors)
                    {
                        var bcs = await _repository.BookCategoryRepository.GetByElement(bookAuthor.BookId, null);
                        if (bcs != null)
                        {
                            foreach (var bc in bcs)
                            {
                                bookCategories.Add(bc);
                            }
                        }
                    }
                }

                if (bookCategories != null)
                {
                    foreach (var bookCategory in bookCategories)
                    {
                        categoryIds.Add(bookCategory.CategoryId);
                    }
                }
            }
            var categories = isAdmin ? await _repository.CategoryRepository.GetAllPaginationForAdmin(key, categoryIds, pageNumber, pageSize, sortField, desc)
                                                       : await _repository.CategoryRepository.GetAllPagination(key, categoryIds, pageNumber, pageSize, sortField, desc);
            return categories.Item1.Count > 0 ? (categories.Item1, categories.Item2) : (null, 0);
        }
        public async Task<List<Category?>> GetTopCategory(int number)
        {
            var categories = new List<Category?>();
            var bookCategories = await _repository.BookCategoryRepository.GetTopCategory(number);
            foreach (var bookCategory in bookCategories)
            {
                var category = await _repository.CategoryRepository.GetByID(bookCategory);
                categories.Add(category);
            }
            return categories;
        }
    }
}
