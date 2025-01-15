using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using AIBookStreet.Repositories.Repositories.Repositories.Repository;
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
    public class BookStoreService : BaseService<BookStore>, IBookStoreService
    {
        private readonly IBookStoreRepository _bookStoreRepository;

        public BookStoreService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(mapper, unitOfWork, httpContextAccessor)
        {
            _bookStoreRepository = unitOfWork.BookStoreRepository;
        }

        public async Task<List<BookStoreModel>> GetAll()
        {
            var bookStores = await _bookStoreRepository.GetAll();

            if (!bookStores.Any())
            {
                return null;
            }

            return _mapper.Map<List<BookStoreModel>>(bookStores);
        }

        public async Task<List<BookStoreModel>?> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var bookStores = await _bookStoreRepository.GetAllPagination(pageNumber, pageSize, sortField, sortOrder);

            if (!bookStores.Any())
            {
                return null;
            }

            return _mapper.Map<List<BookStoreModel>>(bookStores);
        }

        public async Task<BookStoreModel?> GetById(Guid id)
        {
            var bookStore = await _bookStoreRepository.GetById(id);

            if (bookStore == null)
            {
                return null;
            }

            return _mapper.Map<BookStoreModel>(bookStore);
        }

        public async Task<(List<BookStoreModel>?, long)> Search(BookStoreModel bookStoreModel, int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var bookStores = _mapper.Map<BookStore>(bookStoreModel);
            var bookStoresWithTotalOrigin = await _bookStoreRepository.Search(bookStores, pageNumber, pageSize, sortField, sortOrder);

            if (!bookStoresWithTotalOrigin.Item1.Any())
            {
                return (null, bookStoresWithTotalOrigin.Item2);
            }
            var bookStoreModels = _mapper.Map<List<BookStoreModel>>(bookStoresWithTotalOrigin.Item1);

            return (bookStoreModels, bookStoresWithTotalOrigin.Item2);
        }

        public async Task<bool> Add(BookStoreModel bookStoreModel)
        {
            var mappedBookStore = _mapper.Map<BookStore>(bookStoreModel);
            var newBookStore = await SetBaseEntityToCreateFunc(mappedBookStore);
            return await _bookStoreRepository.Add(newBookStore);
        }

        public async Task<bool> Update(BookStoreModel bookStoreModel)
        {
            var existingBookStore = await _bookStoreRepository.GetById(bookStoreModel.Id);

            if (existingBookStore == null)
            {
                return false;
            }

            _mapper.Map(bookStoreModel, existingBookStore);
            var updatedBookStore = await SetBaseEntityToUpdateFunc(existingBookStore);

            return await _bookStoreRepository.Update(updatedBookStore);
        }

        public async Task<bool> Delete(Guid bookStoreId)
        {
            var existingBookStore = await _bookStoreRepository.GetById(bookStoreId);
            if (existingBookStore == null)
            {
                return false;
            }

            var mappedBookStore = _mapper.Map<BookStore>(existingBookStore);
            return await _bookStoreRepository.Delete(mappedBookStore);
        }
    }
}
