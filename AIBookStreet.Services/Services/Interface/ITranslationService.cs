using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface ITranslationService
    {
        Task<string?> TranslateToVietnameseAsync(string text);
        Task<string?> TranslateTextAsync(string text, string targetLanguage, string sourceLanguage = "auto");
    }
} 