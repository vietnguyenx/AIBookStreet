using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IBookCategoryService
    {
        Task<(BookCategory?, long)> AddABookCategory(BookCategoryModel model);
        Task<(long, BookCategory?)> UpdateABookCategory(Guid id, BookCategoryModel model);
        Task<(long, BookCategory?)> DeleteABookCategory(Guid id);
        Task<BookCategory?> GetABookCategoryById(Guid id);
        Task<List<BookCategory>?> GetBookCategoryByElement(Guid? bookID, Guid? categoryID);
        Task<List<BookCategory>?> GetAllActiveBookCategories();
        Task<(List<BookCategory>?, long)> GetAllBookCategoriesPagination(string? key, Guid? bookID, Guid? categoryID, int? pageNumber, int? pageSize, string? sortField, bool? desc);
    }
}
