using Application.Secrets;
using Infrastructure.Severa;

namespace Hangfire.Jobs
{
    public class TestJob(ISecretClient _secretClient, ILogger<TestJob> _logger)
    {
        public void WriteTest()
        {
            _logger.LogInformation("making call");
            var client = new SeveraClient(_secretClient);
            var key = client.GetTokenTest();
            _logger.LogInformation($"got secret for test:{key}");
        }
    }
}
