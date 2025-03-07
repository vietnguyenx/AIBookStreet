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
    public class InventoryService : BaseService<Inventory>, IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(mapper, unitOfWork, httpContextAccessor)
        {
            _inventoryRepository = unitOfWork.InventoryRepository;
        }

        public async Task<List<InventoryModel>> GetAll()
        {
            var inventories = await _inventoryRepository.GetAll();

            if (!inventories.Any())
            {
                return null;
            }

            return _mapper.Map<List<InventoryModel>>(inventories);
        }

        public async Task<List<InventoryModel>?> GetByBookId(Guid bookId)
        {
            var inventories = await _inventoryRepository.GetByBookId(bookId);
            if (!inventories.Any()) return null;
            return _mapper.Map<List<InventoryModel>>(inventories);
        }

        public async Task<List<InventoryModel>?> GetByBookStoreId(Guid bookStoreId)
        {
            var inventories = await _inventoryRepository.GetByBookStoreId(bookStoreId);
            if (!inventories.Any()) return null;
            return _mapper.Map<List<InventoryModel>>(inventories);
        }

        public async Task<bool> Add(InventoryModel inventoryModel)
        {
            var inventory = await _inventoryRepository.GetByBookIdAndBookStoreId(inventoryModel.EntityId, inventoryModel.StoreId);
            if (inventory != null) { return false; }
            var mappedInventory = _mapper.Map<Inventory>(inventoryModel);
            var newInventory = await SetBaseEntityToCreateFunc(mappedInventory);
            return await _inventoryRepository.Add(newInventory);
        }

        public async Task<bool> Delete(Guid bookId, Guid bookStoreId)
        {
            var inventory = await _inventoryRepository.GetByBookIdAndBookStoreId(bookId, bookStoreId);
            if (inventory == null)
            {
                return false;
            }

            var deleteInventory = _mapper.Map<Inventory>(inventory);
            return await _inventoryRepository.Delete(deleteInventory);
        }
    }
}
