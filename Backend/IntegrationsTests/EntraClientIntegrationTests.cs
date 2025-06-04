using Infrastructure.Common;
using Infrastructure.Secrets;
using Infrastructure.Entra;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationsTests
{
    public class EntraClientIntegrationTests
    {
        private EntraClient _sut;

        public EntraClientIntegrationTests()
        {
            var token = Environment.GetEnvironmentVariable("DOPPLER_KEY");
            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");

            var secretHttpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://api.doppler.com/v3/")
            };
            secretHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var retryHandlerLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<RetryHandler>();
            var secretClient = new DopplerClient(secretHttpClient, token, environment);
            var graphHttpClient = new HttpClient(new RetryHandler(new HttpClientHandler(), retryHandlerLogger))
            {
                BaseAddress = new Uri("https://graph.microsoft.com/v1.0/")
            };

            var entraLogger = retryHandlerLogger;
            _sut = new EntraClient(secretClient, graphHttpClient, entraLogger);
        }

        [Fact]
        public async Task EntraClient_GetTokenAsync_ReturnsNonEmptyToken()
        {
            // Act
            var token = await _sut.GetTokenAsync();

            Thread.Sleep(500);

            // Assert
            Assert.False(string.IsNullOrEmpty(token));
        }

        [Fact]
        public async Task EntraClient_GetAllEmployeesAsync_ReturnsNonEmptyList()
        {
            // Act
            var employees = await _sut.GetAllEmployeesAsync();

            Thread.Sleep(500);

            // Assert
            Assert.NotNull(employees);
            Assert.NotEmpty(employees);
        }
    }
}
