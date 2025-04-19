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
    public class StoreRepository : BaseRepository<Store>, IStoreRepository
    {
        private readonly BSDbContext _context;

        public StoreRepository(BSDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Store>> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var queryable = GetQueryable();
            queryable = base.ApplySort(queryable, sortField, sortOrder);

            queryable = GetQueryablePagination(queryable, pageNumber, pageSize);

            return await queryable
                .Include(bs => bs.Images)
                .ToListAsync();
        }

        public async Task<Store?> GetById(Guid id)
        {
            var query = GetQueryable(bs => bs.Id == id);
            var bookStore = await query
                .Include(bs => bs.Images)
                .Include(bs => bs.Inventories)
                .Include(bs => bs.UserStores)
                .Include(bs => bs.Zone)
                .SingleOrDefaultAsync();

            return bookStore;
        }

        public async Task<(List<Store>, long)> SearchPagination(Store store, int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var queryable = GetQueryable()
                .Where(bs => !bs.IsDeleted);
            queryable = base.ApplySort(queryable, sortField, sortOrder);

            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(store.StoreName))
                {
                    queryable = queryable.Where(bs =>
                        EF.Functions.Collate(bs.StoreName, "Latin1_General_CI_AI").Contains(store.StoreName));
                }

                if (!string.IsNullOrEmpty(store.Address))
                {
                    queryable = queryable.Where(bs =>
                        EF.Functions.Collate(bs.Address, "Latin1_General_CI_AI").Contains(store.Address));
                }

                if (!string.IsNullOrEmpty(store.Phone))
                {
                    queryable = queryable.Where(bs =>
                        EF.Functions.Collate(bs.Phone, "Latin1_General_CI_AI").Contains(store.Phone));
                }

                if (!string.IsNullOrEmpty(store.Email))
                {
                    queryable = queryable.Where(bs =>
                        EF.Functions.Collate(bs.Email, "Latin1_General_CI_AI").Contains(store.Email));
                }

                if (!string.IsNullOrEmpty(store.StoreTheme))
                {
                    queryable = queryable.Where(bs =>
                        EF.Functions.Collate(bs.StoreTheme, "Latin1_General_CI_AI").Contains(store.StoreTheme));
                }

                if (!string.IsNullOrEmpty(store.Type))
                {
                    queryable = queryable.Where(bs =>
                        EF.Functions.Collate(bs.Type, "Latin1_General_CI_AI").Contains(store.Type));
                }

                if (store.ZoneId.HasValue)
                {
                    queryable = queryable.Where(bs => bs.ZoneId == store.ZoneId);
                }
            }
            var totalOrigin = queryable.Count();

            queryable = GetQueryablePagination(queryable, pageNumber, pageSize);

            var bookstores = await queryable
                .Include(bs => bs.Images)
                .ToListAsync();

            return (bookstores, totalOrigin);
        }

        public async Task<List<Store>> SearchWithoutPagination(Store store)
        {
            var queryable = GetQueryable()
                .Where(bs => !bs.IsDeleted);

            if (!string.IsNullOrEmpty(store.StoreName))
            {
                queryable = queryable.Where(bs =>
                    EF.Functions.Collate(bs.StoreName, "Latin1_General_CI_AI").Contains(store.StoreName));
            }

            if (!string.IsNullOrEmpty(store.Address))
            {
                queryable = queryable.Where(bs =>
                    EF.Functions.Collate(bs.Address, "Latin1_General_CI_AI").Contains(store.Address));
            }

            if (!string.IsNullOrEmpty(store.Phone))
            {
                queryable = queryable.Where(bs =>
                    EF.Functions.Collate(bs.Phone, "Latin1_General_CI_AI").Contains(store.Phone));
            }

            if (!string.IsNullOrEmpty(store.Email))
            {
                queryable = queryable.Where(bs =>
                    EF.Functions.Collate(bs.Email, "Latin1_General_CI_AI").Contains(store.Email));
            }

            if (!string.IsNullOrEmpty(store.StoreTheme))
            {
                queryable = queryable.Where(bs =>
                    EF.Functions.Collate(bs.StoreTheme, "Latin1_General_CI_AI").Contains(store.StoreTheme));
            }

            if (!string.IsNullOrEmpty(store.Type))
            {
                queryable = queryable.Where(bs =>
                    EF.Functions.Collate(bs.Type, "Latin1_General_CI_AI").Contains(store.Type));
            }
            
            if (store.ZoneId.HasValue)
            {
                queryable = queryable.Where(bs => bs.ZoneId == store.ZoneId);
            }

            return await queryable.Include(bs => bs.Images).ToListAsync();
        }

        public async Task<long> GetTotalCount()
        {
            return await GetQueryable().Where(s => !s.IsDeleted).CountAsync();
        }

        public async Task<List<Store>> GetStoresByMonth(int month, int year)
        {
            return await GetQueryable()
                .Where(s => s.CreatedDate.Month == month &&
                       s.CreatedDate.Year == year &&
                       !s.IsDeleted)
                .ToListAsync();
        }
    }
}
