using Domain.Interfaces.ExternalClients;
using Infrastructure.Common;
using Infrastructure.Entra;
using Infrastructure.Severa.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    public class EntraClientTests
    {
        [Fact]
        public async Task GetTokenAsync_ReturnsNull_WhenSecretClientThrows()
        {
            // Arrange
            var secretMock = new Mock<ISecretClient>();
            secretMock
                .Setup(s => s.GetSecretAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken?>()))
                .ThrowsAsync(new Exception("secret error"));


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


            var loggerMock = new Mock<ILogger<RetryHandler>>();
            var client = new EntraClient(
                secretMock.Object,
                new HttpClient(mockHandler.Object),
                loggerMock.Object);

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(
                () => client.GetTokenAsync());

            // Assert
            Assert.Equal("secret error", ex.Message);
            secretMock.Verify(
                s => s.GetSecretAsync("ENTRA_TENANT", null),
                Times.Once);
        }

        [Fact]
        public async Task GetUsersJsonAsync_ReturnsNull_WhenTokenNull()
        {
            // Arrange
            var secretMock = new Mock<ISecretClient>();
            secretMock
                .Setup(s => s.GetSecretAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken?>()))
                .ThrowsAsync(new Exception("secret error"));



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


            var loggerMock = new Mock<ILogger<RetryHandler>>();
            var client = new EntraClient(
                secretMock.Object,
                new HttpClient(mockHandler.Object),
                loggerMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                () => client.GetAllEmployeesAsync());
            Assert.Equal("secret error", ex.Message);
        }
    }
}
