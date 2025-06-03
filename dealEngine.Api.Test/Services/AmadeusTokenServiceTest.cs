using dealEngine.AmadeusFlightApi.Interfaces;
using dealEngine.AmadeusFlightApi.Services;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace dealEngine.AmadeusFlightApi.Tests.Services
{
    [TestFixture]
    public class AmadeusTokenServiceTest
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private AmadeusSettings _settings;
        private IAmadeusTokenService _amadeusTokenService;

        [SetUp]
        public void Setup()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            
            _httpMessageHandlerMock.Protected()
             .Setup("Dispose", ItExpr.IsAny<bool>());

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);

            _settings = new AmadeusSettings
            {
                ClientId = "test-client-id",
                ClientSecret = "test-client-secret",
                BaseUrl = "https://mock.api"
            };

            var options = Options.Create(_settings);

            _amadeusTokenService = new AmadeusFlightApi.Services.AmadeusTokenService(_httpClient, options);
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

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(fakeJson)
            };

            _httpMessageHandlerMock.Protected()
           .Setup<Task<HttpResponseMessage>>("SendAsync",
               ItExpr.Is<HttpRequestMessage>(req =>
                   req.Method == HttpMethod.Post &&
                   req.RequestUri!.ToString().Contains("/v1/security/oauth2/token")),
               ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(responseMessage)
           .Verifiable();

            // Act
            var token = await _amadeusTokenService.GetTokenAsync();

            // Assert
            Assert.AreEqual(expectedToken, token);
        }

        [Test]
        public async Task GetTokenAsync_ReturnsToken_AndCachesIt()
        {
            var expectedToken = "mock_token";
            var fakeJson = JsonSerializer.Serialize(new
            {
                access_token = expectedToken,
                expires_in = 3600
            });

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(fakeJson)
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString().Contains("/v1/security/oauth2/token")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage)
                .Verifiable();

            // Act
            var token1 = await _amadeusTokenService.GetTokenAsync();
            var token2 = await _amadeusTokenService.GetTokenAsync(); // should use cached token

            // Assert
            Assert.AreEqual("mock_token", token1);
            Assert.AreEqual(token1, token2);

            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(), // only one call to the token endpoint
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public void Constructor_Throws_WhenHttpClientIsNull()
        {
            var options = Options.Create(_settings);
            Assert.Throws<ArgumentNullException>(() => new AmadeusTokenService(null!, options));
        }

        [Test]
        public void Constructor_Throws_WhenOptionsIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AmadeusTokenService(_httpClient, null!));
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }
    } 
}
