using AutoMapper;
using dealEngine.AmadeusFlightApi.Interfaces;
using dealEngine.AmadeusFlightApi.Models;
using dealEngine.AmadeusFlightApi.Models.FligthOffer;
using dealEngine.AmadeusFlightApi.Models.Locations;
using Microsoft.AspNetCore.WebUtilities;
using Polly.Retry;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace dealEngine.AmadeusFlightApi.Services
{
    public class AmadeusService : IAmadeusService
    {

        private readonly HttpClient _httpClient;
        private readonly IAmadeusTokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly string _baseUrl;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        public AmadeusService(IHttpClientFactory clientFactory, IConfiguration config, IAmadeusTokenService tokenService, IMapper mapper, ILogger<AmadeusService> logger)
        {
            _httpClient = clientFactory.CreateClient("AmadeusClient");
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _mapper = mapper;
            _baseUrl = config["Amadeus:BaseUrl"] ?? throw new ArgumentNullException("BaseUrl not configured");

            //_retryPolicy = HttpPolicyExtensions.HandleTransientHttpError()
            //        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            //        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            //        (exception, timeSpan, retryCount, context) =>
            //        {
            //            logger.LogInformation($"---- Retry {retryCount} encountered an error: {exception.Exception.Message}. Waiting {timeSpan} before next retry. ----");
            //        });
        }



        public async Task<List<FlightResult>> SearchFlightsAsync(FlightPreference criteria)
        {
            var token = await _tokenService.GetTokenAsync();

            var queryParams = new Dictionary<string, string>
            {
                {"origin", criteria.Origin.ToUpper()},
                {"oneWay",criteria.OneWay.ToString()},
                {"nonStop", criteria.NonStop.ToString()},
                {"maxPrice", criteria.MaxPrice.ToString()},
                {"viewBy", criteria.ViewBy.ToString().ToUpper()},
            };
            var fullUrl = $"{_baseUrl}/v1/shopping/flight-destinations";

            var url = QueryHelpers.AddQueryString(fullUrl, queryParams);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.amadeus+json"));

            //var response = await _retryPolicy.ExecuteAsync(async () =>
            //{ 
            //    return await _httpClient.SendAsync(request);

            //});
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
            var token = await _tokenService.GetTokenAsync();

            var fullUrl = $"{_baseUrl}/v2/shopping/flight-offers";
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, fullUrl);
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
                        var currency = offer.GetProperty("price").GetProperty("currency").GetString();
                        var price = offer.GetProperty("price").GetProperty("total").GetString();

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
            var token = await _tokenService.GetTokenAsync();

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
