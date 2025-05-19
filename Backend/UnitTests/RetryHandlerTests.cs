using Infrastructure.Severa;
using Microsoft.Extensions.Logging;
using Moq.Protected;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public class RetryHandlerTests
    {
        [Fact]
        public async Task SendAsync_Retries_OnRetriableStatusCode()
        {
            // Arrange
            var retryCount = 0;

            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.InternalServerError));

            var loggerMock = new Mock<ILogger<RetryHandler>>();
            var retryHandler = new RetryHandler(mockHandler.Object, loggerMock.Object);
            var client = new HttpClient(retryHandler);

            // Act
            var response = await client.GetAsync("https://example.com");

            // Assert
            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(4),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.NotFound)]
        public async Task SendAsync_DoesNotRetry_OnNonRetriableStatusCode(HttpStatusCode statusCode)
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(statusCode));

            var loggerMock = new Mock<ILogger<RetryHandler>>();
            var retryHandler = new RetryHandler(mockHandler.Object, loggerMock.Object);
            var client = new HttpClient(retryHandler);

            // Act
            var response = await client.GetAsync("https://example.com");

            // Assert
            Assert.Equal(statusCode, response.StatusCode);
            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
