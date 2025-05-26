using System.Diagnostics;
using System.Threading.Tasks;
using Domain.Interfaces.ExternalClients;
using Microsoft.Extensions.Logging;

namespace Hangfire.Jobs;

public class TestJob(IEntraClient entraClient, ILogger<TestJob> _logger)
{
    public async Task WriteTest()
    {
    }
}
