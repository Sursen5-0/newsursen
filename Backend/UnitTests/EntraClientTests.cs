using Infrastructure.Entra;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Application.Secrets;

namespace UnitTests
{
    public class EntraClientTests
    {
        [Fact]
        public async Task GetTokenAsync_ReturnsNull_WhenSecretClientThrows()
        {
            // Arrange
            var secretMock = new Mock<Application.Secrets.ISecretClient>();
            secretMock
                .Setup(s => s.GetSecretAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken?>()))
                .ThrowsAsync(new Exception("secret error"));

            var loggerMock = new Mock<ILogger<EntraRetryHandler>>();
            var client = new EntraClient(
                secretMock.Object,
                loggerMock.Object);

            // Act
            var token = await client.GetTokenAsync();

            // Assert
            Assert.Null(token);
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

            var loggerMock = new Mock<ILogger<EntraRetryHandler>>();
            var client = new EntraClient(
                secretMock.Object,
                loggerMock.Object);

            // Act
            var json = await client.GetUsersJsonAsync();

            // Assert
            Assert.Null(json);
        }
    }
}
