using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface ICategoryService
    {
        Task<Category?> AddACategory(CategoryModel model);
        Task<(long, Category?)> UpdateACategory(Guid? id, CategoryModel model);
        Task<(long, Category?)> DeleteACategory(Guid id);
        Task<Category?> GetACategoryById(Guid id);
        Task<List<Category>?> GetAllActiveCategories(string? name, Guid? authorId);
        Task<(List<Category>?, long)> GetAllCategoriesPagination(string? key, Guid? authorId, int? pageNumber, int? pageSize, string? sortField, bool? desc);
    }
}
