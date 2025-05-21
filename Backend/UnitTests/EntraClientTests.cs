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

        [Fact]
        public async Task GetTokenAsync_FetchesSecrets_OnEachCall_WhenHttpFails()
        {
            // Arrange
            var secretMock = new Mock<Application.Secrets.ISecretClient>();
            secretMock
                .Setup(s => s.GetSecretAsync("ENTRA_TENANT", It.IsAny<CancellationToken?>()))
                .ReturnsAsync("tid");
            secretMock
                .Setup(s => s.GetSecretAsync("ENTRA_ID", It.IsAny<CancellationToken?>()))
                .ReturnsAsync("cid");
            secretMock
                .Setup(s => s.GetSecretAsync("ENTRA_SECRET", It.IsAny<CancellationToken?>()))
                .ReturnsAsync("csec");

            var loggerMock = new Mock<ILogger<EntraRetryHandler>>();
            var client = new EntraClient(
                secretMock.Object,
                loggerMock.Object);

            // Act
            _ = await client.GetTokenAsync();
            _ = await client.GetTokenAsync();

            // Assert
            secretMock.Verify(
                s => s.GetSecretAsync("ENTRA_TENANT", null),
                Times.Exactly(2));
            secretMock.Verify(
                s => s.GetSecretAsync("ENTRA_ID", null),
                Times.Exactly(2));
            secretMock.Verify(
                s => s.GetSecretAsync("ENTRA_SECRET", null),
                Times.Exactly(2));
        }
    }
}
