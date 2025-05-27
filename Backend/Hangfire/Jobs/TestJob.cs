using System.Diagnostics;
using System.Threading.Tasks;
using Domain.Interfaces.ExternalClients;
using Microsoft.Extensions.Logging;

namespace Hangfire.Jobs;

public class TestJob(IEntraClient entraClient, ILogger<TestJob> _logger)
{
    public async Task WriteTest()
    {
        _logger.LogInformation("making call");

        var json = await entraClient.GetTokenAsync();

        _logger.LogInformation($"got users JSON: {json}");

        Debug.WriteLine("Users JSON:");
        Debug.WriteLine(json);
    }
}
