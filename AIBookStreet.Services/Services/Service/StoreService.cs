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
    public class StoreService : BaseService<Store>, IStoreService
    {
        private readonly IStoreRepository _bookStoreRepository;

        public StoreService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(mapper, unitOfWork, httpContextAccessor)
        {
            _bookStoreRepository = unitOfWork.BookStoreRepository;
        }

        public async Task<List<StoreModel>> GetAll()
        {
            var bookStores = await _bookStoreRepository.GetAll();

            if (!bookStores.Any())
            {
                return null;
            }

            return _mapper.Map<List<StoreModel>>(bookStores);
        }

        public async Task<List<StoreModel>?> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var bookStores = await _bookStoreRepository.GetAllPagination(pageNumber, pageSize, sortField, sortOrder);

            if (!bookStores.Any())
            {
                return null;
            }

            return _mapper.Map<List<StoreModel>>(bookStores);
        }

        public async Task<StoreModel?> GetById(Guid id)
        {
            var bookStore = await _bookStoreRepository.GetById(id);

            if (bookStore == null)
            {
                return null;
            }

            return _mapper.Map<StoreModel>(bookStore);
        }

        public async Task<(List<StoreModel>?, long)> SearchPagination(StoreModel bookStoreModel, int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var bookStores = _mapper.Map<Store>(bookStoreModel);
            var bookStoresWithTotalOrigin = await _bookStoreRepository.SearchPagination(bookStores, pageNumber, pageSize, sortField, sortOrder);

            if (!bookStoresWithTotalOrigin.Item1.Any())
            {
                return (null, bookStoresWithTotalOrigin.Item2);
            }
            var bookStoreModels = _mapper.Map<List<StoreModel>>(bookStoresWithTotalOrigin.Item1);

            return (bookStoreModels, bookStoresWithTotalOrigin.Item2);
        }

        public async Task<List<StoreModel>?> SearchWithoutPagination(StoreModel bookStoreModel)
        {
            var bookStore = _mapper.Map<Store>(bookStoreModel);
            var bookStores = await _bookStoreRepository.SearchWithoutPagination(bookStore);

            if (!bookStores.Any())
            {
                return null;
            }

            return _mapper.Map<List<StoreModel>>(bookStores);
        }


        public async Task<bool> Add(StoreModel bookStoreModel)
        {
            var mappedBookStore = _mapper.Map<Store>(bookStoreModel);
            var newBookStore = await SetBaseEntityToCreateFunc(mappedBookStore);
            return await _bookStoreRepository.Add(newBookStore);
        }

        public async Task<bool> Update(StoreModel bookStoreModel)
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

            var mappedBookStore = _mapper.Map<Store>(existingBookStore);
            return await _bookStoreRepository.Delete(mappedBookStore);
        }
    }
}
