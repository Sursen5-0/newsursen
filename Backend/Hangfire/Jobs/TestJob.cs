using Application.Secrets;

namespace Hangfire.Jobs
{
    public class TestJob(ISecretClient _secretClient, ILogger<TestJob> _logger)
    {
        public void WriteTest()
        {
            _logger.LogInformation("making call");
            var key = _secretClient.GetSecretAsync("test").Result;
            _logger.LogInformation($"got secret for test:{key}");
        }
    }
}
