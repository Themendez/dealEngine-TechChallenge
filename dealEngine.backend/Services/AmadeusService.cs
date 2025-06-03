using AutoMapper;
using dealEngine.AmadeusFlightApi.Interfaces;
using dealEngine.AmadeusFlightApi.Models;
using dealEngine.AmadeusFlightApi.Models.FligthOffer;
using dealEngine.AmadeusFlightApi.Models.Locations;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace dealEngine.AmadeusFlightApi.Services
{
    public class AmadeusService : IAmadeusService
    {

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private string? _token;
        private DateTime _tokenExpiresAt;
        private readonly IMapper _mapper;

        public AmadeusService(HttpClient httpClient, IConfiguration config, IMapper mapper)
        {
            _httpClient = httpClient;
            _config = config;
            _mapper = mapper;
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

            var queryParams = new Dictionary<string, string>
            {
                {"origin", criteria.Origin.ToUpper()}, 
                {"oneWay",criteria.OneWay.ToString()},
                {"nonStop", criteria.NonStop.ToString()},
                {"maxPrice", criteria.MaxPrice.ToString()},
                {"viewBy", criteria.ViewBy.ToString().ToUpper()},
            };

            var url = QueryHelpers.AddQueryString("https://test.api.amadeus.com/v1/shopping/flight-destinations", queryParams);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.amadeus+json"));

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

            return results;
        }

        public async Task<List<FlightOfferResult>> SearchFlightOffersAsync(FlightOfferRequest request)
        {
            var token = await GetTokenAsync();

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://test.api.amadeus.com/v2/shopping/flight-offers");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var results = new List<FlightOfferResult>();
            using var doc = JsonDocument.Parse(content);

            var offers = doc.RootElement.GetProperty("data");

            foreach (var offer in offers.EnumerateArray())
            {
                if (!offer.TryGetProperty("itineraries", out var itineraries)) continue;

                foreach (var itinerary in itineraries.EnumerateArray())
                {
                    if (!itinerary.TryGetProperty("segments", out var segments)) continue;

                    foreach (var segment in segments.EnumerateArray())
                    {
                        var departure = segment.GetProperty("departure").GetProperty("iataCode").GetString();
                        var arrival = segment.GetProperty("arrival").GetProperty("iataCode").GetString();
                        var carrier = segment.GetProperty("carrierCode").GetString();
                        var number = segment.GetProperty("number").GetString();
                        var currency = segment.GetProperty("price").GetProperty("currency").GetString();
                        var price = segment.GetProperty("price").GetProperty("total").GetString();

                        results.Add(new FlightOfferResult
                        {
                            Origin = departure ?? string.Empty,
                            Destination = arrival ?? string.Empty,
                            Airline = carrier ?? string.Empty,
                            FlightNumber = number ?? string.Empty,
                            Price= price ?? string.Empty,
                            Currency= currency ?? string.Empty
                        });
                    }
                }
            }

            return results;
        }

        public async Task<PagedResult<LocationResult>> SearchLocationsAsync(LocationSearchRequest request)
        {
            var token = await GetTokenAsync();

            var queryParams = new Dictionary<string, string>
                {
                    { "subType", request.SubType },
                    { "keyword", request.Keyword },
                    { "page[limit]", request.PageSize.ToString() },
                    { "page[offset]", request.GetOffset().ToString() },
                    { "sort", request.Sort },
                    { "view", request.View },
                    {"countryCode", request.CountryCode ?? string.Empty }
                };

            var url = QueryHelpers.AddQueryString("https://test.api.amadeus.com/v1/reference-data/locations", queryParams);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseString);
            var root = jsonDoc.RootElement;

            var count = root.GetProperty("meta").GetProperty("count").GetInt32();

            var mapped = root
                .GetProperty("data")
                .EnumerateArray()
                .Select(item => _mapper.Map<LocationResult>(item))
                .ToList();

            return new PagedResult<LocationResult>
            {
                Total = count,
                Limit = request.PageSize,
                Offset = request.GetOffset(),
                Data = mapped
            };
        }
    }
}
