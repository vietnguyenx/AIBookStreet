using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface ITranslationService
    {
        Task<string?> TranslateToVietnameseAsync(string text);
    }
} 