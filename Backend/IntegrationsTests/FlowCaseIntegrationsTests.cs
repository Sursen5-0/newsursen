using Domain.Interfaces.ExternalClients;
using Domain.Models;
using Infrastructure.FlowCase;
using Infrastructure.Secrets;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
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
    private readonly Logger<FlowCaseClient> _logger = new Logger<FlowCaseClient>(new LoggerFactory());

    public FlowCaseIntegrationsTests()
    {
        var token = Environment.GetEnvironmentVariable("DOPPLER_KEY");
        var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
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

        _client = new FlowCaseClient(_secretClient, flowCaseHttpClient, _logger);
    }


    [Fact]
    public async Task FetchSkills_FromFlowCase_IsSuccessfull()
    {
        // Arrange
        List<SkillDTO> skills = await _client.GetSkillsFromFlowcaseAsync();

        Assert.All(skills, skill => Assert.NotNull(skill));
        Assert.All(skills, skill => Assert.False(string.IsNullOrWhiteSpace(skill.SkillName)));

    }
}