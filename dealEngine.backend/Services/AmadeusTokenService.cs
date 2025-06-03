using dealEngine.AmadeusFlightApi.Interfaces;
using Microsoft.Extensions.Options;
using System.Runtime;
using System.Text.Json;

namespace dealEngine.AmadeusFlightApi.Services
{
    public class AmadeusTokenService : IAmadeusTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly AmadeusSettings _settings; 
        private string? _token;
        private DateTime _tokenExpiresAt;
        private static readonly SemaphoreSlim _tokenLock = new(1, 1);

        public AmadeusTokenService(HttpClient httpClient, IOptions<AmadeusSettings> amadeusSettings)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _tokenExpiresAt = DateTime.MinValue;
            _settings = amadeusSettings?.Value ?? throw new ArgumentNullException(nameof(amadeusSettings));
        }

        public async Task<string> GetTokenAsync()
        {
            if (!string.IsNullOrEmpty(_token) && DateTime.UtcNow < _tokenExpiresAt)
                return _token!;

            await _tokenLock.WaitAsync();

            try
            {
                if (!string.IsNullOrEmpty(_token) && DateTime.UtcNow < _tokenExpiresAt)
                    return _token!;

                var clientId = _settings.ClientId;
                var clientSecret = _settings.ClientSecret;

                var content = new FormUrlEncodedContent(new[]
                {
                        new KeyValuePair<string, string>("grant_type", "client_credentials"),
                        new KeyValuePair<string, string>("client_id", clientId!),
                        new KeyValuePair<string, string>("client_secret", clientSecret!)
                    });

                var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/v1/security/oauth2/token", content);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var tokenData = JsonSerializer.Deserialize<JsonElement>(json);

                _token = tokenData!.GetProperty("access_token").GetString();
                var expiresIn = tokenData.GetProperty("expires_in").GetInt32();
                _tokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn - 60);

                return _token!;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        public async Task<string> GetTokenAsync(bool forceRefresh)
        {
            if (!forceRefresh && !string.IsNullOrEmpty(_token) && DateTime.UtcNow < _tokenExpiresAt)
                return _token!;

            return await GetTokenAsync();
        }
    }
}
