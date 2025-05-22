using Domain.Interfaces;
using Domain.Interfaces.ExternalClients;
using Infrastructure.Severa;

namespace Hangfire.Jobs
{
    public class TestJob(ISeveraClient severaClient, ILogger<TestJob> _logger)
    {
        public void WriteTest()
        {
            _logger.LogInformation("making call");
            var key = severaClient.GetToken().Result;
            _logger.LogInformation($"got secret for test:{key}");
        }
    }
}
