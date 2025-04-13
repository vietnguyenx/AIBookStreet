using AIBookStreet.Services.Model;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IGoogleBookService
    {
        Task<BookModel?> SearchBookByISBN(string isbn);
    }
} 