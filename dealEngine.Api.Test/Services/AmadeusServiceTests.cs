using AutoMapper;
using dealEngine.AmadeusFlightApi.Controllers;
using dealEngine.AmadeusFlightApi.Interfaces;
using dealEngine.AmadeusFlightApi.Models;
using dealEngine.AmadeusFlightApi.Models.FligthOffer;
using dealEngine.AmadeusFlightApi.Models.Locations;
using dealEngine.AmadeusFlightApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace dealEngine.AmadeusFlightApi.Tests.Services
{
    [TestFixture]
    public class AmadeusServiceTests
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private AmadeusService _service;
        private IConfiguration _config;
        private HttpClient _httpClient;
        private Mock<IMapper> _mapperMock;

        private Mock<IAmadeusService> _amadeusMock;
        private Mock<IAmadeusTokenService> _tokenServiceMock;
        private Mock<ILogger<AmadeusService>> _loggerMock;
        private FlightsController _controller;

        [SetUp]
        public void Setup()
        {
            var settings = new Dictionary<string, string>
                {
                    { "Amadeus:ClientId", "test-client-id" },
                    { "Amadeus:ClientSecret", "test-secret" },
                    { "Amadeus:BaseUrl", "https://test.api.amadeus.com/" }

                };

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpMessageHandlerMock.Protected()
             .Setup("Dispose", ItExpr.IsAny<bool>());

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://test.api.amadeus.com/")
            };

            _mapperMock = new Mock<IMapper>();
            _tokenServiceMock = new Mock<IAmadeusTokenService>();
            _loggerMock = new Mock<ILogger<AmadeusService>>();

            _service = new AmadeusService(_httpClient, _config, _tokenServiceMock.Object, _mapperMock.Object, _loggerMock.Object);

            _amadeusMock = new Mock<IAmadeusService>();
            _controller = new FlightsController(_amadeusMock.Object, _tokenServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient?.Dispose();
        }

        [Test]
        public async Task SearchFlights_ReturnsOk_WithResults()
        {
            // Arrange
            var expectedResults = new List<FlightResult>
        {
            new FlightResult { Destination = "NYC", Price = "150" },
            new FlightResult { Destination = "LON", Price = "180" }
        };

            _amadeusMock
     .Setup(s => s.SearchFlightsAsync(It.Is<FlightPreference>(
         p => p.Origin == "PAR" &&
              p.MaxPrice == 200 &&
              p.ViewBy == ViewByEnum.country)))
     .ReturnsAsync(expectedResults);

            var input = new FlightPreference
            {
                Origin = "PAR",
                MaxPrice = 200,
                ViewBy = ViewByEnum.country
            };

            // Act
            var result = await _controller.SearchFlights(input) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            var response = result.Value as ApiResponse<List<FlightResult>>;
            Assert.IsTrue(response.Success);
            Assert.AreEqual(2, response.Data.Count);
        }


        [Test]
        public async Task SearchLocationsAsync_ReturnsExpectedPagedResult()
        {

            var request = new LocationSearchRequest
            {
                SubType = "CITY",
                Keyword = "MUC",
                PageSize = 10,
                PageNumber = 1,
                Sort = "analytics.travelers.score",
                View = "FULL",
                CountryCode = "DE"
            };

            string fakeToken = "fake_token";
            string jsonResponse = @"
            {
              ""meta"": { ""count"": 1 },
              ""data"": [
                {
                  ""iataCode"": ""MUC"",
                  ""name"": ""Munich International""
                }
              ]
            }";

            _tokenServiceMock
                .Setup(x => x.GetTokenAsync())
                .ReturnsAsync(fakeToken);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString().Contains("reference-data/locations")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            _mapperMock
                .Setup(m => m.Map<LocationResult>(It.IsAny<JsonElement>()))
                .Returns(new LocationResult
                {
                    IataCode = "MUC",
                    Name = "Munich International"
                });

            // Act
            var result = await _service.SearchLocationsAsync(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Total);
            Assert.AreEqual(1, result.Data.Count);
            Assert.AreEqual("MUC", result.Data[0].IataCode);
        }

        [Test]
        public async Task SearchFlightOffersAsync_ReturnsFlightOfferResults()
        {
            var dummyToken = "test-token";
            _tokenServiceMock = new Mock<IAmadeusTokenService>();
            _tokenServiceMock.Setup(x => x.GetTokenAsync()).ReturnsAsync(dummyToken);

            var fakeJsonResponse = @"{
            ""data"": [
                {
                    ""itineraries"": [
                        {
                            ""segments"": [
                                {
                                    ""departure"": { ""iataCode"": ""NYC"" },
                                    ""arrival"": { ""iataCode"": ""MAD"" },
                                    ""carrierCode"": ""IB"",
                                    ""number"": ""625""
                                }
                            ]
                        }
                    ],
                    ""price"": {
                        ""total"": ""1234.56"",
                        ""currency"": ""USD""
                    }
                }
            ]
        }";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(fakeJsonResponse, Encoding.UTF8, "application/json")
            });


            var request = new FlightOfferRequest
            {
                CurrencyCode = "USD",
                OriginDestinations = new List<OriginDestination>
            {
                new OriginDestination
                {
                    Id = "1",
                    OriginLocationCode = "NYC",
                    DestinationLocationCode = "MAD",
                    DepartureDateTimeRange = new DepartureDateTimeRange
                    {
                        Date = "2025-11-01",
                        Time = "10:00:00"
                    }
                }
            },
                Travelers = new List<Traveler>
            {
                new Traveler { Id = "1", TravelerType = "ADULT" }
            },
                Sources = new List<string> { "GDS" },
                SearchCriteria = new SearchCriteria
                {
                    MaxFlightOffers = 2,
                    FlightFilters = new FlightFilters
                    {
                        CabinRestrictions = new List<CabinRestriction>
                    {
                        new CabinRestriction
                        {
                            Cabin = "BUSINESS",
                            Coverage = "MOST_SEGMENTS",
                            OriginDestinationIds = new List<string> { "1" }
                        }
                    }
                    }
                }
            };

            // Act
            var result = await _service.SearchFlightOffersAsync(request);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Origin, Is.EqualTo("NYC"));
            Assert.That(result[0].Destination, Is.EqualTo("MAD"));
            Assert.That(result[0].Airline, Is.EqualTo("IB"));
            Assert.That(result[0].FlightNumber, Is.EqualTo("625"));
            Assert.That(result[0].Price, Is.EqualTo("1234.56"));
            Assert.That(result[0].Currency, Is.EqualTo("USD"));
        }
    }

}


