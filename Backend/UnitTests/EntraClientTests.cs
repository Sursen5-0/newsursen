using Infrastructure.Entra;
using Microsoft.Extensions.Logging;
using Moq;
using System;
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
            var secretMock = new Mock<Application.Secrets.ISecretClient>();
            secretMock
                .Setup(s => s.GetSecretAsync(It.IsAny<string>(), It.IsAny<CancellationToken?>()))
                .ThrowsAsync(new Exception("secret error"));

            var loggerMock = new Mock<ILogger<EntraRetryHandler>>();
            var client = new EntraClient(secretMock.Object, loggerMock.Object);

            var token = await client.GetTokenAsync();
            Assert.Null(token);
            secretMock.Verify(s => s.GetSecretAsync("ENTRA_TENANT", null), Times.Once);
        }

        [Fact]
        public async Task GetUsersJsonAsync_ReturnsNull_WhenTokenNull()
        {
            var secretMock = new Mock<Application.Secrets.ISecretClient>();
            secretMock
                .Setup(s => s.GetSecretAsync(It.IsAny<string>(), It.IsAny<CancellationToken?>()))
                .ThrowsAsync(new Exception("secret error"));

            var loggerMock = new Mock<ILogger<EntraRetryHandler>>();
            var client = new EntraClient(secretMock.Object, loggerMock.Object);

            var json = await client.GetUsersJsonAsync();
            Assert.Null(json);
        }

        [Fact]
        public async Task GetTokenAsync_FetchesSecrets_OnEachCall_WhenHttpFails()
        {
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
            var client = new EntraClient(secretMock.Object, loggerMock.Object);

            // Call twice; since HTTP will fail internally, _token stays null, so secrets are fetched twice
            _ = await client.GetTokenAsync();
            _ = await client.GetTokenAsync();

            // Each secret should have been requested twice
            secretMock.Verify(s => s.GetSecretAsync("ENTRA_TENANT", null), Times.Exactly(2));
            secretMock.Verify(s => s.GetSecretAsync("ENTRA_ID", null), Times.Exactly(2));
            secretMock.Verify(s => s.GetSecretAsync("ENTRA_SECRET", null), Times.Exactly(2));
        }
    }
}
