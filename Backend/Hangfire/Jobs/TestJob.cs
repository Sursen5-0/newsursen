using Application.Secrets;
using Infrastructure.Severa;

namespace Hangfire.Jobs
{
    public class TestJob(SeveraClient severaClient, ILogger<TestJob> _logger)
    {
        public void WriteTest()
        {
            _logger.LogInformation("making call");
            var key = severaClient.GetToken().Result;
            _logger.LogInformation($"got secret for test:{key}");
        }
    }
}
