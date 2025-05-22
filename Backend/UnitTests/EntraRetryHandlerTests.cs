using Infrastructure.Entra;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    public class EntraRetryHandlerTests
    {
        [Fact]
        public async Task SendAsync_Retries_OnRetriableStatusCode()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var loggerMock = new Mock<ILogger<EntraRetryHandler>>();
            var retryHandler = new EntraRetryHandler(mockHandler.Object, loggerMock.Object);
            var client = new HttpClient(retryHandler);

            var response = await client.GetAsync("https://example.com");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(4),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.NotFound)]
        public async Task SendAsync_DoesNotRetry_OnNonRetriableStatusCode(HttpStatusCode code)
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(code));

            var loggerMock = new Mock<ILogger<EntraRetryHandler>>();
            var retryHandler = new EntraRetryHandler(mockHandler.Object, loggerMock.Object);
            var client = new HttpClient(retryHandler);

            var response = await client.GetAsync("https://example.com");
            Assert.Equal(code, response.StatusCode);
            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SendAsync_Retries_OnException_ThenSucceeds()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network fail"))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var loggerMock = new Mock<ILogger<EntraRetryHandler>>();
            var retryHandler = new EntraRetryHandler(mockHandler.Object, loggerMock.Object);
            var client = new HttpClient(retryHandler);

            var response = await client.GetAsync("https://example.com");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(2),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
