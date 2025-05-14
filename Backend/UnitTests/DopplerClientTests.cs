using System.Net.Http.Json;
using System.Net;
using System.Threading;
using Infrastructure.Secrets;
using Moq;
using Moq.Protected;
using Microsoft.Extensions.Logging;

namespace UnitTests
{
    public class DopplerClientTests
    {
        [Fact]
        public async Task GetSecretAsync_ReturnsExpectedSecret()
        {
            // Arrange
            var expectedSecret = "API_KEY_VALUE";
            var key = "API_KEY";

            var response = new SecretResponse
            {
                Secret = new Secret{ Value = expectedSecret },
                Success = true
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

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://api.doppler.com/v3/")
            };

            var logger = Mock.Of<ILogger<DopplerClient>>();
            var dopplerClient = new DopplerClient(httpClient, logger);

            // Act
            var result = await dopplerClient.GetSecretAsync(key);

            // Assert
            Assert.Equal(expectedSecret, result);
        }
    }
}
