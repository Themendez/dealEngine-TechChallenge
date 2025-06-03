using AutoMapper;
using dealEngine.AmadeusFlightApi.Controllers;
using dealEngine.AmadeusFlightApi.Interfaces;
using dealEngine.AmadeusFlightApi.Models;
using dealEngine.AmadeusFlightApi.Models.Locations;
using dealEngine.AmadeusFlightApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System.Net;
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
        private FlightsController _controller;

        [SetUp]
        public void Setup()
        {
            var settings = new Dictionary<string, string>
                {
                    { "Amadeus:ClientId", "test-client-id" },
                    { "Amadeus:ClientSecret", "test-secret" }
                };

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            //_httpClient = new HttpClient(new MockHttpMessageHandler());
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpMessageHandlerMock.Protected()
             .Setup("Dispose", ItExpr.IsAny<bool>());

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://test.api.amadeus.com/")
            };

            _mapperMock = new Mock<IMapper>();


            _tokenServiceMock = new Mock<IAmadeusTokenService>();

            _service = new AmadeusService(_httpClient, _config, _tokenServiceMock.Object, _mapperMock.Object);

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


    }

}
