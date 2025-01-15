using AIBookStreet.Repositories.Data;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Repository
{
    public class InventoryRepository : BaseRepository<Inventory>, IInventoryRepository
    {
        private readonly BSDbContext _context;

        public InventoryRepository(BSDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Inventory?>> GetByBookId(Guid bookId)
        {
            var query = GetQueryable(i => i.BookId == bookId);
            return await query
                .Include(i => i.Book)
                .Include(i => i.BookStore)
                .ToListAsync();
        }

        public async Task<List<Inventory?>> GetByBookStoreId(Guid bookStoreId)
        {
            var query = GetQueryable(i => i.BookStoreId == bookStoreId);
            return await query
                .Include(i => i.Book)
                .Include(i => i.BookStore)
                .ToListAsync();
        }

        public async Task<Inventory?> GetByBookIdAndBookStoreId(Guid bookId, Guid bookStoreId)
        {
            Inventory inventory = await _context.Inventories.Where(x => x.BookId == bookId && x.BookStoreId == bookStoreId)
                .Include(i => i.Book)
                .Include(i => i.BookStore)
                .SingleOrDefaultAsync();
            return inventory;
        }
    }
}
