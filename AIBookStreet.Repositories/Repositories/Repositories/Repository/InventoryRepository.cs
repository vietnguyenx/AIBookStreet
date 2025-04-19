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
            var query = GetQueryable(i => i.EntityId == bookId);
            return await query
                .Include(i => i.Book)
                .Include(i => i.Souvenir)
                .Include(i => i.Store)
                .ToListAsync();
        }

        public async Task<List<Inventory?>> GetByStoreId(Guid storeId)
        {
            var query = GetQueryable(i => i.StoreId == storeId);
            return await query
                .Include(i => i.Book)
                .Include(i => i.Souvenir)
                .Include(i => i.Store)
                .ToListAsync();
        }

        public async Task<Inventory?> GetByBookIdAndStoreId(Guid? bookId, Guid storeId)
        {
            Inventory inventory = await _context.Inventories.Where(x => x.EntityId == bookId && x.StoreId == storeId)
                .Include(i => i.Book)
                .Include(i => i.Souvenir)
                .Include(i => i.Store)
                .SingleOrDefaultAsync();
            return inventory;
        }

        public async Task<Inventory?> GetByID(Guid? id)
        {
            var query = GetQueryable(at => at.Id == id);
            var author = await query
                .Include(i => i.Book)
                .Include(i => i.Souvenir)
                .Include(i => i.Store)
                .SingleOrDefaultAsync();

            return author;
        }
    }
}
