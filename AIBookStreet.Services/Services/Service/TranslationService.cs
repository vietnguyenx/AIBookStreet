using System;
using System.Threading.Tasks;
using AIBookStreet.Services.Services.Interface;
using Google.Cloud.Translation.V2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIBookStreet.Services.Services.Service
{
    public class TranslationService : ITranslationService
    {
        private readonly TranslationClient _translationClient;
        private readonly ILogger<TranslationService> _logger;

        public TranslationService(IConfiguration configuration, ILogger<TranslationService> logger)
        {
            _logger = logger;
            try
            {
                var apiKey = configuration["GoogleTranslation:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogWarning("Google Translation API key is not configured. Translation service will not work.");
                    _translationClient = null;
                }
                else
                {
                    _translationClient = TranslationClient.CreateFromApiKey(apiKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Google Translation client");
                _translationClient = null;
            }
        }

        public async Task<string?> TranslateToVietnameseAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            if (_translationClient == null)
            {
                _logger.LogWarning("Translation client is not initialized. Returning original text.");
                return text;
            }

            try
            {
                var response = await Task.FromResult(_translationClient.TranslateText(
                    text,
                    LanguageCodes.Vietnamese,
                    LanguageCodes.English));

                return response.TranslatedText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Translation failed for text: {Text}", text);
                return text; // Return original text in case of failure
            }
        }
    }
} 