﻿using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface IAuthorRepository : IBaseRepository<Author>
    {
        Task<List<Author>> GetAll();
        Task<(List<Author>, long)> GetAllPagination(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc);
        Task<Author?> GetByID(Guid id);
    }
}
