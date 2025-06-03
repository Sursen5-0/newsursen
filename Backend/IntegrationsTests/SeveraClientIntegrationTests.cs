using Infrastructure.Common;
using Infrastructure.Secrets;
using Infrastructure.Severa;
using Infrastructure.Severa.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationsTests
{
    public class SeveraClientIntegrationTests
    {
        private SeveraClient _sut;
        public SeveraClientIntegrationTests()
        {
            var token = Environment.GetEnvironmentVariable("DOPPLER_KEY");
            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");

            var secretHttpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://api.doppler.com/v3/")
            };
            secretHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var retryHandlerLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<RetryHandler>();
            var severaLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<SeveraClient>();
            var secretClient = new DopplerClient(secretHttpClient, token, environment);
            var severaHttpClient = new HttpClient(new RetryHandler(new HttpClientHandler(), retryHandlerLogger))
            {
                BaseAddress = new Uri("https://api.severa.visma.com/rest-api/v1.0/")
            };

            _sut = new SeveraClient(secretClient, severaHttpClient, severaLogger);
        }
        [Fact]
        public async Task SeveraClient_CallingGetWorkContractByUserIdWithNoId_ReturnsError()
        {
            // Act
            var result = await _sut.GetWorkContractByUserId(Guid.Empty);

            // Assert
            Assert.Null(result);
        }
        [Fact]
        public async Task SeveraClient_CallingGetWorkContractByUserIdWithValidId_WorkContract()
        {
            // Act 
            var result = await _sut.GetWorkContractByUserId(Guid.Parse("96354066-dc12-2680-48b5-5f70dda84508"));

            // Sleeping to ensure that apithrottling wont happen
            Thread.Sleep(500);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task SeveraClient_GetUserByEmailWithNoEmail_ReturnsError()
        {
            // Act 
            var result = await _sut.GetUserByEmail(null);

            // Sleeping to ensure that apithrottling wont happen
            Thread.Sleep(500);

            // Assert
            Assert.Null(result);
        }
        [Fact]
        public async Task SeveraClient_GetUserByEmailWithInvalidEmail_ReturnsError()
        {
            // Act 
            var result = await _sut.GetUserByEmail("not an email");

            // Sleeping to ensure that apithrottling wont happen
            Thread.Sleep(500);

            // Assert
            Assert.Null(result);
        }
        [Fact]
        public async Task SeveraClient_GetUserByEmailWithValidEmail_ReturnsValidUser()
        {
            //Arrange
            var email = "bjorn.andersen@twoday.com";


            // Act 
            var result = await _sut.GetUserByEmail(email);

            // Sleeping to ensure that apithrottling wont happen
            Thread.Sleep(500);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(result.Email, email);
            Assert.NotEqual(result.Id, Guid.Empty);
        }

        [Fact]
        public async Task SeveraClient_GetAbsence_ReturnsResponse()
        {
            // Act
            var result = await _sut.GetAbsence(maxpages: 3);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task SeveraClient_GetProjects_ReturnsResponse()
        {
            // Act
            var result = await _sut.GetProjects(maxpages: 3);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task SeveraClient_GetPhases_ReturnsResponse()
        {
            // Arrange
            // Amgen ID's SeveraIds
            var projectGuids = new List<Guid>
            {
                new Guid("96b5a7d5-593a-6398-0134-4c36271c8cb5"),
                new Guid("ecd59f4f-141d-28fc-4061-511c3ffbac60")
            };

            // Act
            var result = await _sut.GetPhases(projectGuids);

            // Assert
            Assert.NotNull(result);
        }

    }
}
