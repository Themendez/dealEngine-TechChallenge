using dealEngine.AmadeusFlightApi.Interfaces;
using System.Text.Json;

namespace dealEngine.AmadeusFlightApi.Services
{
    public class AmadeusTokenService : IAmadeusTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private string? _token;
        private DateTime _tokenExpiresAt;

        public AmadeusTokenService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _tokenExpiresAt = DateTime.MinValue; 
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

        public async Task<string> GetTokenAsync(bool forceRefresh)
        {
            if (!forceRefresh && !string.IsNullOrEmpty(_token) && DateTime.UtcNow < _tokenExpiresAt)
                return _token!;

            return await GetTokenAsync();
        }
    }
}
