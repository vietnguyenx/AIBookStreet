using AIBookStreet.Repositories.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Utils
{
    public class PersonApiResponse
    {
        [JsonPropertyName("current_page")]
        public int CurrentPage { get; set; }

        [JsonPropertyName("data")]
        public List<PersonApiModel> Data { get; set; }

        [JsonPropertyName("stats")]
        public PersonStats Stats { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("total_results")]
        public int TotalResults { get; set; }
    }

    public class PersonApiModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; }

        [JsonPropertyName("features")]
        public string Features { get; set; }

        [JsonPropertyName("first_seen")]
        public DateTime FirstSeen { get; set; }

        [JsonPropertyName("last_seen")]
        public DateTime LastSeen { get; set; }

        [JsonPropertyName("daily_appearances")]
        public int DailyAppearances { get; set; }

        [JsonPropertyName("total_appearances")]
        public int TotalAppearances { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    public class PersonStats
    {
        [JsonPropertyName("daily_appearances")]
        public Dictionary<string, int> DailyAppearances { get; set; }

        [JsonPropertyName("female")]
        public int Female { get; set; }

        [JsonPropertyName("male")]
        public int Male { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("total_appearances")]
        public int TotalAppearances { get; set; }
    }

    public static class PersonApiHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string _apiUrl = "https://unitrack-production.up.railway.app/api/persons/";
        private const int MaxRetries = 3;
        private const int RetryDelayMilliseconds = 1000;

        public static async Task<List<Person>> FetchPersonsFromApi()
        {
            var allPersons = new List<Person>();
            var currentPage = 1;
            var totalPages = 1;
            var retryCount = 0;

            do
            {
                try
                {
                    var pageUrl = $"{_apiUrl}?page={currentPage}";
                    var response = await _httpClient.GetAsync(pageUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        if (retryCount < MaxRetries)
                        {
                            retryCount++;
                            await Task.Delay(RetryDelayMilliseconds * retryCount); // Exponential backoff
                            continue;
                        }
                        else
                        {
                            throw new HttpRequestException($"Failed to fetch data after {MaxRetries} retries. Status code: {response.StatusCode}");
                        }
                    }

                    var apiResponse = await response.Content.ReadFromJsonAsync<PersonApiResponse>();

                    if (apiResponse?.Data == null || !apiResponse.Data.Any())
                        break;

                    // Update total pages on first request
                    if (currentPage == 1)
                    {
                        totalPages = apiResponse.TotalPages;
                    }

                    var persons = apiResponse.Data.Select(p => new Person
                    {
                        ExternalId = p.Id,
                        Gender = p.Gender,
                        Features = p.Features,
                        FirstSeen = p.FirstSeen,
                        LastSeen = p.LastSeen,
                        DailyAppearances = p.DailyAppearances,
                        TotalAppearances = p.TotalAppearances,
                        ExternalCreatedAt = p.CreatedAt,
                        ExternalUpdatedAt = p.UpdatedAt
                    }).ToList();

                    allPersons.AddRange(persons);
                    currentPage++;
                    retryCount = 0; // Reset retry count for next page
                }
                catch (Exception ex)
                {
                    if (retryCount < MaxRetries)
                    {
                        retryCount++;
                        await Task.Delay(RetryDelayMilliseconds * retryCount);
                        continue;
                    }
                    else
                    {
                        Console.WriteLine($"Error fetching persons from API: {ex.Message}");
                        throw; // Re-throw the exception after all retries are exhausted
                    }
                }
            } while (currentPage <= totalPages);

            return allPersons;
        }
    }
}