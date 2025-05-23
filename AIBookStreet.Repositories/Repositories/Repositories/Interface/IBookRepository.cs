﻿using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface IBookRepository : IBaseRepository<Book>
    {
        Task<List<Book>> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<Book?> GetById(Guid id);
        Task<(List<Book>, long)> SearchPagination(Book book, DateTime? startDate, DateTime? endDate, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<List<Book>> SearchWithoutPagination(Book book, DateTime? startDate, DateTime? endDate, decimal? minPrice, decimal? maxPrice);
    }
}
