using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace AIBookStreet.Services.Services.Service
{
    public class GoogleBookService : IGoogleBookService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GoogleBookService> _logger;

        public GoogleBookService(HttpClient httpClient, IConfiguration configuration, ILogger<GoogleBookService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GoogleBooks:ApiKey"];
            _logger = logger;
        }

        public async Task<BookModel?> SearchBookByISBN(string isbn)
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

                var bookModel = new BookModel
                {
                    ISBN = isbn,
                    Title = book.Title,
                    PublicationDate = publishedDate,
                    Price = book.Price,
                    Languages = book.Language,
                    Description = book.Description,                                     
                    Size = book.Dimensions?.Height != null
                        ? $"{book.Dimensions.Height}x{book.Dimensions.Width}x{book.Dimensions.Thickness}"
                        : null,
                    Status = "New",
                    BookAuthors = book.Authors?.Select(author => new BookAuthorModel { AuthorName = author }).ToList(),
                    BookCategories = book.Categories?.Select(category => new BookCategoryModel { CategoryName = category }).ToList(),
                    Publisher = !string.IsNullOrEmpty(book.Publisher) 
                        ? new PublisherModel { PublisherName = book.Publisher } 
                        : null
                };
                
                if (book.ImageLinks != null)
                {
                    bookModel.Images = new List<Image>();
                    
                    // Add thumbnail as main image
                    if (!string.IsNullOrEmpty(book.ImageLinks.Thumbnail))
                    {
                        bookModel.Images.Add(new Image
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
                        bookModel.Images.Add(new Image
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