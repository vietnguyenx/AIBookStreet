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

        public async Task<List<InventoryModel>?> GetByBookId(Guid bookId)
        {
            var inventories = await _inventoryRepository.GetByBookId(bookId);
            if (!inventories.Any()) return null;
            return _mapper.Map<List<InventoryModel>>(inventories);
        }

        public async Task<List<InventoryModel>?> GetByStoreId(Guid storeId)
        {
            var inventories = await _inventoryRepository.GetByStoreId(storeId);
            if (!inventories.Any()) return null;
            return _mapper.Map<List<InventoryModel>>(inventories);
        }

        public async Task<bool> Add(InventoryModel inventoryModel)
        {
            var inventory = await _inventoryRepository.GetByBookIdAndStoreId(inventoryModel.EntityId, inventoryModel.StoreId);
            if (inventory != null) { return false; }
            var mappedInventory = _mapper.Map<Inventory>(inventoryModel);
            var newInventory = await SetBaseEntityToCreateFunc(mappedInventory);
            return await _inventoryRepository.Add(newInventory);
        }

        public async Task<bool> Delete(Guid bookId, Guid storeId)
        {
            var inventory = await _inventoryRepository.GetByBookIdAndStoreId(bookId, storeId);
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
                var inventory = await _inventoryRepository.GetByBookIdAndStoreId(book.Id, storeId);
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
