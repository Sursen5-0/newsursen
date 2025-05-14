using System.Net.Http.Json;
using System.Net;
using System.Threading;
using Infrastructure.Secrets;
using Moq;
using Moq.Protected;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace UnitTests
{
    public class DopplerClientTests
    {
        [Fact]
        public async Task GetSecretAsync_ReturnsSecretValue_WhenResponseIsOk()
        {
            // Arrange
            var expectedSecret = "API_KEY_VALUE";
            var key = "API_KEY";

            var response = new SecretResponse
            {
                Secret = new Secret { Value = expectedSecret }
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(msg => msg.RequestUri!.ToString().Contains(key.ToUpper())),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(response)
                });

            var httpClient = new HttpClient(handlerMock.Object);

            var dopplerClient = new DopplerClient(httpClient, "TEST", "TEST");

            // Act
            var result = await dopplerClient.GetSecretAsync(key);

            // Assert
            Assert.Equal(expectedSecret, result);
        }
        [Fact]
        public void Constructor_ThrowsException_WhenEnvVarMissing()
        {
            // Arrange
            var client = new HttpClient();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new DopplerClient(client, null, null));
        }
        [Theory]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.BadGateway)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.GatewayTimeout)]
        public async Task GetSecretAsync_ThrowsExpectedException_ForErrorStatus(HttpStatusCode statusCode)
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = statusCode });

            var client = new HttpClient(mockHandler.Object);

            var dopplerClient = new DopplerClient(client, "TEST", "TEST");
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => dopplerClient.GetSecretAsync("somekey"));
        }
    }
}
