﻿using AIBookStreet.Repositories.Data;
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
    public class PublisherRepository : BaseRepository<Publisher>, IPublisherRepository
    {
        public PublisherRepository(BSDbContext context) : base(context)
        {
        }

        public async Task<List<Publisher>> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var queryable = GetQueryable();
            queryable = base.ApplySort(queryable, sortField, sortOrder);

            queryable = GetQueryablePagination(queryable, pageNumber, pageSize);

            return await queryable
                .Include(p => p.Images)
                .Include(m => m.Manager)
                .Include(p => p.Books)
                .ToListAsync();
        }

        public async Task<Publisher?> GetById(Guid id)
        {
            var query = GetQueryable(m => m.Id == id);
            var publisher = await query
                .Include(p => p.Images)
                .Include(m => m.Manager)
                .Include(p => p.Books)
                .SingleOrDefaultAsync();

            return publisher;
        }

        public async Task<(List<Publisher>, long)> Search(Publisher publisher, int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var queryable = GetQueryable();
            queryable = base.ApplySort(queryable, sortField, sortOrder);

            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(publisher.PublisherName))
                {
                    queryable = queryable.Where(m => m.PublisherName.ToLower().Trim().StartsWith(publisher.PublisherName.ToLower().Trim()));
                }

                if (!string.IsNullOrEmpty(publisher.Website))
                {
                    queryable = queryable.Where(m => m.Website.ToLower().Trim().StartsWith(publisher.Website.ToLower().Trim()));
                }

            }

            var totalOrigin = queryable.Count();
            queryable = GetQueryablePagination(queryable, pageNumber, pageSize);

            var publishers = await queryable
                .Include(p => p.Images)
                .Include(p => p.Manager)
                .Include(p => p.Books)
                .ToListAsync();
            return (publishers, totalOrigin);
        }
    }
}
