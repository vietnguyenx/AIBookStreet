using AIBookStreet.Services.Model;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IGoogleBookService
    {
        Task<GoogleBookResponseModel?> SearchBookByISBN(string isbn);
    }
} 