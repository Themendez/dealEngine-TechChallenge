using dealEngine.AmadeusFlightApi.Controllers;
using dealEngine.AmadeusFlightApi.Interfaces;
using dealEngine.AmadeusFlightApi.Models;
using dealEngine.AmadeusFlightApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using RichardSzalay.MockHttp;

namespace dealEngine.AmadeusFlightApi.Tests.Services
{
    [TestFixture]
    public class AmadeusServiceTests
    {
        private AmadeusService _service;
        private IConfiguration _config;
        private HttpClient _httpClient;

        private Mock<IAmadeusService> _amadeusMock;
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

            _httpClient = new HttpClient(new MockHttpMessageHandler()); // Puedes simular respuestas
            _service = new AmadeusService(_httpClient, _config);

            _amadeusMock = new Mock<IAmadeusService>();

            _controller = new FlightsController(_amadeusMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient?.Dispose();
        }

        [Test]
        public async Task GetTokenAsync_ShouldReturnToken()
        {
            var token = await _service.GetTokenAsync();
            Assert.IsNotNull(token);
        }

        [Test]
        public async Task SearchFlights_ReturnsOk_WithResults()
        {
            // Arrange
            var expectedResults = new List<FlightResult>
        {
            new FlightResult { Destination = "NYC", Price = 150 },
            new FlightResult { Destination = "LON", Price = 180 }
        };

            _amadeusMock
            .Setup(s => s.SearchFlightsAsync(new FlightPreference { Origin = "PAR", MaxPrice = 200, SortBy = "price" }))
                .ReturnsAsync(expectedResults);

            var input = new FlightPreference
            {
                Origin = "PAR",
                MaxPrice = 200,
                SortBy = "price"
            };

            // Act
            var result = await _controller.SearchFlights(input) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            var response = result.Value as ApiResponse<List<FlightResult>>;
            Assert.IsTrue(response.Success);
            Assert.AreEqual(2, response.Data.Count);
        }


    }

}
