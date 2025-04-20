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
        private readonly IBookRepository _bookRepository;

        public InventoryService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(mapper, unitOfWork, httpContextAccessor)
        {
            _inventoryRepository = unitOfWork.InventoryRepository;
            _bookRepository = unitOfWork.BookRepository;
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

        public async Task<List<InventoryModel>?> GetByEntityId(Guid entityId)
        {
            var inventories = await _inventoryRepository.GetByEntityId(entityId);
            if (!inventories.Any()) return null;
            return _mapper.Map<List<InventoryModel>>(inventories);
        }

        public async Task<List<InventoryModel>?> GetByStoreId(Guid storeId)
        {
            var inventories = await _inventoryRepository.GetByStoreId(storeId);
            if (!inventories.Any()) return null;
            return _mapper.Map<List<InventoryModel>>(inventories);
        }

        public async Task<List<InventoryModel>?> GetBooksByStoreId(Guid storeId)
        {
            var inventories = await _inventoryRepository.GetBooksByStoreId(storeId);
            if (!inventories.Any()) return null;
            return _mapper.Map<List<InventoryModel>>(inventories);
        }

        public async Task<List<InventoryModel>?> GetSouvenirsByStoreId(Guid storeId)
        {
            var inventories = await _inventoryRepository.GetSouvenirsByStoreId(storeId);
            if (!inventories.Any()) return null;
            return _mapper.Map<List<InventoryModel>>(inventories);
        }

        public async Task<(bool, string)> Add(InventoryModel inventoryModel)
        {
            try
            {
                // Validate input parameters
                if (inventoryModel.EntityId == null || inventoryModel.EntityId == Guid.Empty)
                {
                    return (false, "Entity ID cannot be empty");
                }

                if (inventoryModel.StoreId == Guid.Empty)
                {
                    return (false, "Store ID cannot be empty");
                }

                if (inventoryModel.Quantity < 0)
                {
                    return (false, "Quantity cannot be negative");
                }

                // Check if inventory already exists
                var inventory = await _inventoryRepository.GetByEntityIdAndStoreId(inventoryModel.EntityId, inventoryModel.StoreId);
                if (inventory != null)
                {
                    return (false, "Inventory already exists for this item in the store. Use Update instead.");
                }

                // Set IsInStock based on quantity
                inventoryModel.IsInStock = inventoryModel.Quantity > 0;

                // Map and create new inventory
                var mappedInventory = _mapper.Map<Inventory>(inventoryModel);
                var newInventory = await SetBaseEntityToCreateFunc(mappedInventory);
                
                var result = await _inventoryRepository.Add(newInventory);
                if (!result)
                {
                    return (false, "Failed to add inventory");
                }

                return (true, $"Successfully added inventory with quantity: {inventoryModel.Quantity}");
            }
            catch (Exception ex)
            {
                return (false, $"Error adding inventory: {ex.Message}");
            }
        }

        public async Task<(bool, string)> Update(Guid entityId, Guid storeId, int quantity)
        {
            try
            {
                if (entityId == Guid.Empty)
                {
                    return (false, "Entity ID cannot be empty");
                }

                if (storeId == Guid.Empty)
                {
                    return (false, "Store ID cannot be empty");
                }

                if (quantity < 0)
                {
                    return (false, "Quantity cannot be negative");
                }

                // Find inventory for this book/entity and store
                var inventory = await _inventoryRepository.GetByEntityIdAndStoreId(entityId, storeId);
                if (inventory == null)
                {
                    return (false, "Inventory not found for this item in the store");
                }

                // Update inventory quantity
                inventory.Quantity = quantity;

                // Update isInStock status based on quantity
                inventory.IsInStock = quantity > 0;

                // Update the last updated information
                inventory = await SetBaseEntityToUpdateFunc(inventory);

                var result = await _inventoryRepository.Update(inventory);
                if (!result)
                {
                    return (false, "Failed to update inventory");
                }

                return (true, $"Successfully updated inventory. New quantity: {inventory.Quantity}");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating inventory: {ex.Message}");
            }
        }

        public async Task<bool> Delete(Guid entityId, Guid storeId)
        {
            var inventory = await _inventoryRepository.GetByEntityIdAndStoreId(entityId, storeId);
            if (inventory == null)
            {
                return false;
            }

            var deleteInventory = _mapper.Map<Inventory>(inventory);
            return await _inventoryRepository.Delete(deleteInventory);
        }

        public async Task<(bool, string)> UpdateQuantityByISBN(string ISBN, Guid storeId, int quantity)
        {
            try
            {
                if (string.IsNullOrEmpty(ISBN))
                {
                    return (false, "Book code cannot be empty");
                }

                if (storeId == Guid.Empty)
                {
                    return (false, "Store ID cannot be empty");
                }

                if (quantity <= 0)
                {
                    return (false, "Quantity must be greater than zero");
                }

                // Find the book by code
                var books = await _bookRepository.SearchWithoutPagination(new Book { ISBN = ISBN }, null, null, null, null);
                if (books == null || !books.Any())
                {
                    return (false, $"Book with code {ISBN} not found");
                }

                var book = books.First();
                
                // Find inventory for this book and store
                var inventory = await _inventoryRepository.GetByEntityIdAndStoreId(book.Id, storeId);
                if (inventory == null)
                {
                    return (false, $"Inventory not found for book ISBN {ISBN} in this store");
                }

                // Check if we have enough books in stock
                if (inventory.Quantity < quantity)
                {
                    return (false, $"Not enough books in stock. Current quantity: {inventory.Quantity}");
                }

                // Update inventory quantity
                inventory.Quantity -= quantity;
                
                // Update isInStock status if quantity reaches zero
                if (inventory.Quantity == 0)
                {
                    inventory.IsInStock = false;
                }

                var result = await _inventoryRepository.Update(inventory);
                if (!result)
                {
                    return (false, "Failed to update inventory");
                }

                return (true, $"Successfully updated inventory. New quantity: {inventory.Quantity}");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating inventory: {ex.Message}");
            }
        }
    }
}
