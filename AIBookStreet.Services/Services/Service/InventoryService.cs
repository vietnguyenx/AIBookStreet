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
                    return (false, "Sản phẩm không được để trống");
                }

                if (inventoryModel.StoreId == Guid.Empty)
                {
                    return (false, "Vui lòng chọn cửa hàng");
                }

                if (inventoryModel.Quantity < 0)
                {
                    return (false, "Số lượng không được âm");
                }

                // Check if inventory already exists
                var inventory = await _inventoryRepository.GetByEntityIdAndStoreId(inventoryModel.EntityId, inventoryModel.StoreId);
                if (inventory != null)
                {
                    return (false, "Đã có hàng tồn kho cho mặt hàng này trong cửa hàng");
                }

                // Set IsInStock based on quantity
                inventoryModel.IsInStock = inventoryModel.Quantity > 0;

                // Map and create new inventory
                var mappedInventory = _mapper.Map<Inventory>(inventoryModel);
                var newInventory = await SetBaseEntityToCreateFunc(mappedInventory);
                
                var result = await _inventoryRepository.Add(newInventory);
                if (!result)
                {
                    return (false, "Không thêm được hàng tồn kho");
                }

                return (true, $"Đã thêm hàng tồn kho thành công với số lượng: {inventoryModel.Quantity}");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi thêm hàng tồn kho: {ex.Message}");
            }
        }

        public async Task<(bool, string)> Update(Guid entityId, Guid storeId, int quantity)
        {
            try
            {
                if (entityId == Guid.Empty)
                {
                    return (false, "Sản phẩm không được để trống");
                }

                if (storeId == Guid.Empty)
                {
                    return (false, "Vui lòng chọn cửa hàng");
                }

                if (quantity < 0)
                {
                    return (false, "Số lượng không được âm");
                }

                // Find inventory for this book/entity and store
                var inventory = await _inventoryRepository.GetByEntityIdAndStoreId(entityId, storeId);
                if (inventory == null)
                {
                    return (false, "Không tìm thấy hàng tồn kho cho mặt hàng này trong cửa hàng");
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
                    return (false, "Không cập nhật được hàng tồn kho");
                }

                return (true, $"Đã cập nhật hàng tồn kho thành công. Số lượng mới: {inventory.Quantity}");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật hàng tồn kho: {ex.Message}");
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
                    return (false, "Mã ISBN không được để trống");
                }

                if (storeId == Guid.Empty)
                {
                    return (false, "Vui lòng chọn cửa hàng");
                }

                if (quantity <= 0)
                {
                    return (false, "Số lượng phải lớn hơn 0");
                }

                // Find the book by code
                var books = await _bookRepository.SearchWithoutPagination(new Book { ISBN = ISBN }, null, null, null, null);
                if (books == null || !books.Any())
                {
                    return (false, $"Không tìm thấy sách có mã {ISBN}");
                }

                var book = books.First();
                
                // Find inventory for this book and store
                var inventory = await _inventoryRepository.GetByEntityIdAndStoreId(book.Id, storeId);
                if (inventory == null)
                {
                    return (false, $"Không tìm thấy hàng tồn kho cho sách ISBN {ISBN} trong cửa hàng này");
                }

                // Check if we have enough books in stock
                if (inventory.Quantity < quantity)
                {
                    return (false, $"Không đủ sách trong kho. Số lượng hiện tại: {inventory.Quantity}");
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
                    return (false, "Không cập nhật được hàng tồn kho");
                }

                return (true, $"Đã cập nhật hàng tồn kho thành công. Số lượng mới: {inventory.Quantity}");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật hàng tồn kho: {ex.Message}");
            }
        }
    }
}
