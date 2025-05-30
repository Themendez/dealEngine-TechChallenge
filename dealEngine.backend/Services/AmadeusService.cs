using dealEngine.AmadeusFlightApi.Interfaces;
using dealEngine.AmadeusFlightApi.Models;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace dealEngine.AmadeusFlightApi.Services
{
    public class AmadeusService : IAmadeusService
    {

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private string? _token;
        private DateTime _tokenExpiresAt;

        public AmadeusService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string> GetTokenAsync()
        {
            if (!string.IsNullOrEmpty(_token) && DateTime.UtcNow < _tokenExpiresAt)
                return _token!;

            var clientId = _config["Amadeus:ClientId"];
            var clientSecret = _config["Amadeus:ClientSecret"];

            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", clientId!),
            new KeyValuePair<string, string>("client_secret", clientSecret!)
        });

            var response = await _httpClient.PostAsync("https://test.api.amadeus.com/v1/security/oauth2/token", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<JsonElement>(json);

            _token = tokenData!.GetProperty("access_token").GetString();
            var expiresIn = tokenData.GetProperty("expires_in").GetInt32();
            _tokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn - 60);

            return _token!;
        }

        public async Task<List<FlightResult>> SearchFlightsAsync(FlightPreference criteria)
        {
            var token = await GetTokenAsync();

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://test.api.amadeus.com/v1/shopping/flight-destinations?origin={criteria.Origin}&maxPrice={criteria.MaxPrice}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonDocument.Parse(json).RootElement.GetProperty("data");

            var results = data.EnumerateArray().Select(d => new FlightResult
            {
                Origin = d.GetProperty("origin").GetString()!,
                Destination = d.GetProperty("destination").GetString()!,
                Price = d.GetProperty("price").GetProperty("total").GetString()!
            }).ToList();

            return criteria.SortBy switch
            {
                "destination" => results.OrderBy(r => r.Destination).ToList(),
                _ => results.OrderBy(r => r.Price).ToList()
            };
        }
    }
}
