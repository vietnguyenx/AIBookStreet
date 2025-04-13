using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

                DateTime? publishedDate = null;
                if (DateTime.TryParse(book.PublishedDate, out var parsedDate))
                {
                    publishedDate = parsedDate;
                }

                return new BookModel
                {
                    ISBN = isbn,
                    Title = book.Title,
                    Description = book.Description,
                    Languages = book.Language,
                    PublicationDate = publishedDate,
                    Price = null,
                    Size = book.Dimensions?.Height != null
                        ? $"{book.Dimensions.Height}x{book.Dimensions.Width}x{book.Dimensions.Thickness}"
                        : null,
                    Status = "Available",
                    //BookAuthors = book.Authors?.Select(author => new BookAuthorModel { AuthorName = author }).ToList(),
                    //BookCategories = book.Categories?.Select(category => new BookCategoryModel { CategoryName = category }).ToList()
                };
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
        public List<string> Authors { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public string PublishedDate { get; set; }
        public List<string> Categories { get; set; }
        public Dimensions Dimensions { get; set; }
    }

    public class Dimensions
    {
        public string Height { get; set; }
        public string Width { get; set; }
        public string Thickness { get; set; }
    }
} 