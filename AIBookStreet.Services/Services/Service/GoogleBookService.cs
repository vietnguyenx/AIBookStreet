using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace AIBookStreet.Services.Services.Service
{
    public class GoogleBookService : IGoogleBookService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GoogleBookService> _logger;
        private readonly ITranslationService _translationService;
        private static readonly Regex _vietnamesePattern = new Regex(@"[àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđÀÁẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬÈÉẺẼẸÊẾỀỂỄỆÌÍỈĨỊÒÓỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢÙÚỦŨỤƯỨỪỬỮỰỲÝỶỸỴĐ]", RegexOptions.Compiled);

        public GoogleBookService(HttpClient httpClient, IConfiguration configuration, ILogger<GoogleBookService> logger, ITranslationService translationService)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GoogleBooks:ApiKey"];
            _logger = logger;
            _translationService = translationService;
        }

        public async Task<GoogleBookResponseModel?> SearchBookByISBN(string isbn)
        {
            try
            {
                _logger.LogInformation($"Searching Google Books API for ISBN: {isbn}");
                _logger.LogInformation($"Using API Key: {_apiKey}");

                var url = $"https://www.googleapis.com/books/v1/volumes?q=isbn:{isbn}&key={_apiKey}";
                _logger.LogInformation($"Request URL: {url}");

                var response = await _httpClient.GetAsync(url);
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Response Status: {response.StatusCode}");
                _logger.LogInformation($"Response Content: {responseContent}");

                response.EnsureSuccessStatusCode();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<GoogleBooksResponse>(responseContent, options);

                if (result?.Items == null || !result.Items.Any())
                {
                    _logger.LogWarning($"No books found for ISBN: {isbn}");
                    return null;
                }

                var book = result.Items.First().VolumeInfo;
                _logger.LogInformation($"Found book: {book.Title}");
                _logger.LogInformation($"Book has image links: {book.ImageLinks != null}");
                if (book.ImageLinks != null)
                {
                    _logger.LogInformation($"Thumbnail URL: {book.ImageLinks.Thumbnail}");
                    _logger.LogInformation($"SmallThumbnail URL: {book.ImageLinks.SmallThumbnail}");
                }

                DateTime? publishedDate = null;
                if (!string.IsNullOrEmpty(book.PublishedDate))
                {
                    if (DateTime.TryParseExact(book.PublishedDate, new[] { "yyyy-MM-dd", "yyyy-MM", "yyyy" },
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out var parsedDate))
                    {
                        publishedDate = parsedDate;
                    }
                    else
                    {
                        _logger.LogWarning($"Unable to parse PublishedDate: {book.PublishedDate}");
                    }
                }

                // Xác định ngôn ngữ cần dịch dựa vào trường languages hoặc title
                string? targetLanguage = null;
                bool shouldTranslate = false;

                // Ưu tiên kiểm tra trường languages trước
                if (!string.IsNullOrEmpty(book.Language))
                {
                    var language = book.Language.ToLower();
                    if (language.Contains("vi") || language.Contains("vietnamese"))
                    {
                        targetLanguage = "vi";
                        shouldTranslate = true;
                        _logger.LogInformation($"Book language is Vietnamese ({book.Language}). Will translate all book information to Vietnamese.");
                    }
                    else if (language.Contains("en") || language.Contains("english"))
                    {
                        targetLanguage = "en";
                        shouldTranslate = true;
                        _logger.LogInformation($"Book language is English ({book.Language}). Will translate all book information to English.");
                    }
                    // Có thể thêm các ngôn ngữ khác ở đây
                }
                else
                {
                    // Nếu không có trường languages, dùng logic cũ (kiểm tra title)
                    bool isTitleVietnamese = !string.IsNullOrEmpty(book.Title) && IsVietnameseText(book.Title);
                    if (isTitleVietnamese)
                    {
                        targetLanguage = "vi";
                        shouldTranslate = true;
                        _logger.LogInformation("No language field found. Book title contains Vietnamese characters. Will translate to Vietnamese.");
                    }
                }

                // Dịch các thông tin của sách nếu cần
                string? translatedTitle = book.Title;
                string? translatedDescription = book.Description;
                List<BookAuthorModel>? translatedAuthors = null;
                PublisherModel? translatedPublisher = null;
                List<BookCategoryModel>? translatedCategories = null;

                if (shouldTranslate && !string.IsNullOrEmpty(targetLanguage))
                {
                    try
                    {
                        // Dịch title nếu cần
                        if (!string.IsNullOrEmpty(book.Title) && 
                            (targetLanguage == "vi" ? !IsVietnameseText(book.Title) : IsVietnameseText(book.Title)))
                        {
                            translatedTitle = await _translationService.TranslateTextAsync(book.Title, targetLanguage);
                        }

                        // Dịch description nếu cần
                        if (!string.IsNullOrEmpty(book.Description) && 
                            (targetLanguage == "vi" ? !IsVietnameseText(book.Description) : IsVietnameseText(book.Description)))
                        {
                            translatedDescription = await _translationService.TranslateTextAsync(book.Description, targetLanguage);
                        }                    

                        // Dịch authors nếu có
                        if (book.Authors?.Any() == true)
                        {
                            translatedAuthors = new List<BookAuthorModel>();
                            foreach (var author in book.Authors)
                            {
                                string translatedAuthorName = author;
                                if (targetLanguage == "vi" ? !IsVietnameseText(author) : IsVietnameseText(author))
                                {
                                    translatedAuthorName = await _translationService.TranslateTextAsync(author, targetLanguage) ?? author;
                                }
                                translatedAuthors.Add(new BookAuthorModel { AuthorName = translatedAuthorName });
                            }
                        }

                        // Dịch publisher nếu có
                        if (!string.IsNullOrEmpty(book.Publisher))
                        {
                            string translatedPublisherName = book.Publisher;
                            if (targetLanguage == "vi" ? !IsVietnameseText(book.Publisher) : IsVietnameseText(book.Publisher))
                            {
                                translatedPublisherName = await _translationService.TranslateTextAsync(book.Publisher, targetLanguage) ?? book.Publisher;
                            }
                            translatedPublisher = new PublisherModel { PublisherName = translatedPublisherName };
                        }

                        // Dịch categories nếu có
                        if (book.Categories?.Any() == true)
                        {
                            translatedCategories = new List<BookCategoryModel>();
                            foreach (var category in book.Categories)
                            {
                                string translatedCategoryName = category;
                                if (targetLanguage == "vi" ? !IsVietnameseText(category) : IsVietnameseText(category))
                                {
                                    translatedCategoryName = await _translationService.TranslateTextAsync(category, targetLanguage) ?? category;
                                }
                                translatedCategories.Add(new BookCategoryModel { CategoryName = translatedCategoryName });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to translate book information. Using original data.");
                        // Nếu dịch thất bại, sử dụng dữ liệu gốc
                        translatedTitle = book.Title;
                        translatedDescription = book.Description;
                        translatedAuthors = book.Authors?.Select(author => new BookAuthorModel { AuthorName = author }).ToList();
                        translatedPublisher = !string.IsNullOrEmpty(book.Publisher) 
                            ? new PublisherModel { PublisherName = book.Publisher } 
                            : null;
                        translatedCategories = book.Categories?.Select(category => new BookCategoryModel { CategoryName = category }).ToList();
                    }
                }
                else
                {
                    // Không cần dịch, sử dụng dữ liệu gốc
                    translatedAuthors = book.Authors?.Select(author => new BookAuthorModel { AuthorName = author }).ToList();
                    translatedPublisher = !string.IsNullOrEmpty(book.Publisher) 
                        ? new PublisherModel { PublisherName = book.Publisher } 
                        : null;
                    translatedCategories = book.Categories?.Select(category => new BookCategoryModel { CategoryName = category }).ToList();
                }

                var bookModel = new GoogleBookResponseModel
                {
                    ISBN = isbn,
                    Title = translatedTitle,
                    PublicationDate = publishedDate,
                    Price = book.Price,
                    Languages = book.Language,
                    Description = translatedDescription,                                     
                    Size = book.Dimensions?.Height != null
                        ? $"{book.Dimensions.Height}x{book.Dimensions.Width}x{book.Dimensions.Thickness}"
                        : null,
                    Status = "New",
                    BookAuthors = translatedAuthors,
                    BookCategories = translatedCategories,
                    Publisher = translatedPublisher,
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                };
                
                if (book.ImageLinks != null)
                {
                    bookModel.Images = new List<AIBookStreet.Repositories.Data.Entities.Image>();
                    
                    // Add thumbnail as main image
                    if (!string.IsNullOrEmpty(book.ImageLinks.Thumbnail))
                    {
                        bookModel.Images.Add(new AIBookStreet.Repositories.Data.Entities.Image
                        {
                            Url = book.ImageLinks.Thumbnail,
                            Type = "book_main",
                            AltText = book.Title,
                            EntityId = Guid.Empty,
                            CreatedBy = "System",
                            CreatedDate = DateTime.UtcNow,
                            IsDeleted = false
                        });
                        _logger.LogInformation($"Added main image: {book.ImageLinks.Thumbnail}");
                    }

                    // Add small thumbnail as additional image
                    if (!string.IsNullOrEmpty(book.ImageLinks.SmallThumbnail))
                    {
                        bookModel.Images.Add(new AIBookStreet.Repositories.Data.Entities.Image
                        {
                            Url = book.ImageLinks.SmallThumbnail,
                            Type = "book_additional",
                            AltText = book.Title,
                            EntityId = Guid.Empty,
                            CreatedBy = "System",
                            CreatedDate = DateTime.UtcNow,
                            IsDeleted = false
                        });
                        _logger.LogInformation($"Added additional image: {book.ImageLinks.SmallThumbnail}");
                    }

                    if (!bookModel.Images.Any())
                    {
                        _logger.LogWarning("No valid image URLs found in Google Books response");
                    }
                }
                else
                {
                    _logger.LogWarning("No image links found in Google Books response");
                }

                return bookModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching Google Books API for ISBN: {isbn}");
                return null;
            }
        }

        /// <summary>
        /// Kiểm tra văn bản có chứa ký tự tiếng Việt không
        /// </summary>
        private bool IsVietnameseText(string text)
        {
            return _vietnamesePattern.IsMatch(text);
        }
    }

    public class GoogleBooksResponse
    {
        public List<GoogleBookItem> Items { get; set; }
    }

    public class GoogleBookItem
    {
        public VolumeInfo VolumeInfo { get; set; }
    }

    public class VolumeInfo
    {
        public string Title { get; set; }
        public string PublishedDate { get; set; }
        public decimal Price { get; set; }
        public string Language { get; set; }
        public string Description { get; set; }
        public string Size { get; set; }
        public string Status { get; set; }
        public List<string> Authors { get; set; }
        public List<string> Categories { get; set; }
        public Dimensions Dimensions { get; set; }
        public string Publisher { get; set; }
        public ImageLinks ImageLinks { get; set; }
    }

    public class ImageLinks
    {
        public string SmallThumbnail { get; set; }
        public string Thumbnail { get; set; }
    }

    public class Dimensions
    {
        public string Height { get; set; }
        public string Width { get; set; }
        public string Thickness { get; set; }
    }
} 