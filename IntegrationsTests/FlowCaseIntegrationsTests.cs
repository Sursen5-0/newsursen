using Domain.Interfaces.ExternalClients;
using Infrastructure.FlowCase;
using Infrastructure.Secrets;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationsTests;

public class FlowCaseIntegrationsTests
{
    private readonly FlowCaseClient _client;
    private readonly DopplerClient _secretClient;

    public FlowCaseIntegrationsTests()
    {
        var token = Environment.GetEnvironmentVariable("doppler_key", EnvironmentVariableTarget.Machine);
        var environment = Environment.GetEnvironmentVariable("Environment", EnvironmentVariableTarget.Machine);
        var secretHttpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.doppler.com/v3/"),
            DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
        };
        _secretClient = new DopplerClient(secretHttpClient, token, environment);
        var flowCaseHttpClient = new HttpClient
        {
            BaseAddress = new Uri("https://twoday.flowcase.com")
        };

        _client = new FlowCaseClient(_secretClient, flowCaseHttpClient);
    }

    [Fact]
    public async Task TestFlowCaseClient()
    {
        // Arrange
        var email = "bjorn.andersen@twoday.com";

        // Act
        var response = await _client.GetUser(email); //Used a random used endpoint for testing

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

}