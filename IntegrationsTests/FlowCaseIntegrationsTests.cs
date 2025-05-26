using Application.Secrets;
using Infrastructure.FlowCase;
using Infrastructure.Secrets;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Infrastructure.FlowCase;
using Application.Secrets;

namespace IntegrationsTests;

public class FlowCaseIntegrationsTests
{
    private readonly HttpClient _client;
    private readonly ISecretClient _secretClient; // Mock or implement this in your test setup

    public FlowCaseIntegrationsTests(ISecretClient secretClient)
    {
        _secretClient = secretClient;
        _client = new HttpClient
        {
            BaseAddress = new Uri("https://twoday.flowcase.com")
        };
    }


    [Fact]
    public async Task TestFlowCaseClient()
    {
        // Arrange
        var flowCaseClient = new FlowCaseClient(_secretClient, _client);
        var apiKey = await flowCaseClient.GetApiKey();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var email = "bjorn.andersen@twoday.com";
        var uri = $"/api/v1/users/find?email={WebUtility.UrlEncode(email)}";

        // Act
        var response = await _client.GetAsync(uri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}