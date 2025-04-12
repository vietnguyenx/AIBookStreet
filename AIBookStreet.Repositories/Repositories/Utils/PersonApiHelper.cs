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
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("persons")]
        public List<PersonApiModel> Persons { get; set; }

        [JsonPropertyName("stats")]
        public PersonStats Stats { get; set; }

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

        public static async Task<List<Person>> FetchPersonsFromApi()
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiUrl);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<PersonApiResponse>();

                if (apiResponse?.Persons == null || !apiResponse.Persons.Any())
                    return new List<Person>();

                return apiResponse.Persons.Select(p => new Person
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
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error fetching persons from API: {ex.Message}");
                return new List<Person>();
            }
        }
    }
} 