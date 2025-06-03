using AutoMapper;
using dealEngine.AmadeusFlightApi.Interfaces;
using dealEngine.AmadeusFlightApi.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;

namespace dealEngine.AmadeusFlightApi.Tests.Services
{
    [TestFixture]
    public class AmadeusTokenService
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private Mock<IConfiguration> _configurationMock;
        private IAmadeusTokenService _amadeusTokenService;
        private Mock<IMapper> _mapperMock;

        [SetUp]
        public void Setup()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            
            _httpMessageHandlerMock.Protected()
             .Setup("Dispose", ItExpr.IsAny<bool>());

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock.Setup(x => x["Amadeus:ClientId"]).Returns("test_client_id");
            _configurationMock.Setup(x => x["Amadeus:ClientSecret"]).Returns("test_client_secret");

            _amadeusTokenService = new AmadeusFlightApi.Services.AmadeusTokenService(_httpClient, _configurationMock.Object);
        }

        [Test]
        public async Task GetTokenAsync_ReturnsNewToken_WhenNoCachedToken()
        {
            var expectedToken = "mock_token";
            var fakeJson = JsonSerializer.Serialize(new
            {
                access_token = expectedToken,
                expires_in = 3600
            });

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.AbsoluteUri.Contains("oauth2/token")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(fakeJson, Encoding.UTF8, "application/json")
                });

            // Act
            var token = await _amadeusTokenService.GetTokenAsync();

            // Assert
            Assert.AreEqual(expectedToken, token);
        }

        [Test]
        public async Task GetTokenAsync_UsesCachedToken_WhenValid()
        {
          
            await GetTokenAsync_ReturnsNewToken_WhenNoCachedToken(); // se cachea

            var token = await _amadeusTokenService.GetTokenAsync(); // segunda llamada usa el caché

            Assert.AreEqual("mock_token", token);

            // Verify solo una llamada HTTP ocurrió
            _httpMessageHandlerMock.Protected()
                .Verify<Task<HttpResponseMessage>>("SendAsync",
                    Times.Once(),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>());


        }


        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }
    } 
}
