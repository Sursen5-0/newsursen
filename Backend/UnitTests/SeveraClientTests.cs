using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Domain.Interfaces.ExternalClients;
using Infrastructure.Severa;
using Infrastructure.Severa.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;
namespace UnitTests
{
    public class SeveraClientTests
    {
        private static Mock<ISecretClient> secretClientMock = null!;
        private static Mock<ILogger<SeveraClient>> secretClientLogMock = null!;
        public SeveraClientTests()
        {
            secretClientLogMock = new Mock<ILogger<SeveraClient>>();
            secretClientMock = new Mock<ISecretClient>();
            secretClientMock.Setup(s => s.GetSecretAsync("SEVERA_CLIENT_ID", It.IsAny<CancellationToken?>())).ReturnsAsync("test-client-id");
            secretClientMock.Setup(s => s.GetSecretAsync("SEVERA_CLIENT_SECRET", It.IsAny<CancellationToken?>())).ReturnsAsync("test-client-secret");

        }
        [Fact]
        public async Task GetToken_ReturnsAccessToken_WhenResponseIsSuccessful()
        {
            // Arrange
            var expectedToken = "mocked-access-token";
            var tokenModel = new TokenReturnModel { AccessToken = expectedToken };
            var jsonContent = JsonSerializer.Serialize(tokenModel);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(mockHandler.Object);
            httpClient.BaseAddress = new Uri("https://www.test.com/");
            var severaClient = new SeveraClient(secretClientMock.Object, httpClient, secretClientLogMock.Object);

            // Act
            var token = await severaClient.GetToken();

            // Assert
            Assert.Equal(expectedToken, token);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.NotFound)]
        public async Task GetToken_ThrowsException_OnExpectedHttpErrors(HttpStatusCode statusCode)
        {
            // Arrange
            var responseMessage = new HttpResponseMessage(statusCode);

            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(mockHandler.Object);
            httpClient.BaseAddress = new Uri("https://www.test.com/");

            var secretClientMock = new Mock<ISecretClient>();
            secretClientMock.Setup(s => s.GetSecretAsync(It.IsAny<string>(), It.IsAny<CancellationToken?>()))
                .ReturnsAsync("dummy");

            var client = new SeveraClient(secretClientMock.Object, httpClient, secretClientLogMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => client.GetToken());
        }

        [Fact]
        public async Task GetToken_UsesCachedToken_OnSubsequentCalls()
        {
            // Arrange
            var token = new TokenReturnModel { AccessToken = "once-token" };
            var json = JsonSerializer.Serialize(token);

            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            httpClient.BaseAddress = new Uri("https://www.test.com/");

            var secretClientMock = new Mock<ISecretClient>();
            secretClientMock.Setup(s => s.GetSecretAsync(It.IsAny<string>(), It.IsAny<CancellationToken?>())).ReturnsAsync("dummy");

            var client = new SeveraClient(secretClientMock.Object, httpClient, secretClientLogMock.Object);

            // Act
            var firstToken = await client.GetToken();
            var secondToken = await client.GetToken();

            // Assert
            Assert.Equal("once-token", firstToken);
            Assert.Equal(firstToken, secondToken);
            mockHandler.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }
    }
}