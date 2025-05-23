﻿using AIBookStreet.Repositories.Data.Entities;
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
            try
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
            } catch
            {
                throw;
            }
        }
        public async Task<(long, BookCategory?)> UpdateABookCategory(BookCategoryModel model)
        {
            try
            {
                var existed = await _repository.BookCategoryRepository.GetByElement(model.BookId, model.CategoryId);
                if (existed == null)
                {
                    return (1, null); //khong ton tai
                }
                if (existed[0].IsDeleted)
                {
                    return (3, null);
                }
                existed[0].BookId = model.BookId;
                existed[0].CategoryId = model.CategoryId;
                existed[0] = await SetBaseEntityToUpdateFunc(existed[0]);
                return await _repository.BookCategoryRepository.Update(existed[0]) ? (2, existed[0]) //update thanh cong
                                                                              : (3, null);       //update fail
            } catch
            {
                throw;
            }
        }
        public async Task<(long, BookCategory?)> DeleteABookCategory(BookCategoryModel model)
        {
            try
            {
                var existed = await _repository.BookCategoryRepository.GetByElement(model.BookId, model.CategoryId);
                if (existed == null)
                {
                    return (1, null); //khong ton tai
                }
                existed[0] = await SetBaseEntityToUpdateFunc(existed[0]);
                return await _repository.BookCategoryRepository.Delete(existed[0]) ? (2, existed[0]) //delete thanh cong
                                                                              : (3, null);       //delete fail
            } catch
            {
                throw;
            }
        }
        public async Task<BookCategory?> GetABookCategoryById(Guid id)
        {
            try
            {
                return await _repository.BookCategoryRepository.GetByID(id);
            } catch
            {
                throw;
            }
        }
        public async Task<List<BookCategory>?> GetAllActiveBookCategories()
        {
            try
            {
                var bookCategories = await _repository.BookCategoryRepository.GetAll();
                return bookCategories.Count == 0 ? null : bookCategories;
            } catch
            {
                throw;
            }
        }
        public async Task<(List<BookCategory>?, long)> GetAllBookCategoriesPagination(string? key, Guid? bookID, Guid? categoryID, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            try
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
                var bookCategories = isAdmin ? await _repository.BookCategoryRepository.GetAllPaginationForAdmin(key, bookID, categoryID, pageNumber, pageSize, sortField, desc)
                                                                : await _repository.BookCategoryRepository.GetAllPagination(key, bookID, categoryID, pageNumber, pageSize, sortField, desc);
                return bookCategories.Item1.Count() > 0 ? (bookCategories.Item1, bookCategories.Item2) : (null, 0);
            } catch
            {
                throw;
            }
        }
        public async Task<List<BookCategory>?> GetBookCategoryByElement(Guid? bookID, Guid? categoryID)
        {
            try
            {
                return await _repository.BookCategoryRepository.GetByElement(bookID, categoryID);
            } catch
            {
                throw;
            }
        }
    }
}
