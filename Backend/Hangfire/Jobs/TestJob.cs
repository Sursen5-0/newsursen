using Application.Secrets;

namespace Hangfire.Jobs
{
    public class TestJob
    {
        private ISecretClient _secretClient;
        private ILogger<TestJob> _logger;
        public TestJob(ISecretClient secretClient, ILogger<TestJob> logger)
        {
            _secretClient = secretClient;
            _logger = logger;
        }
        public void WriteTest()
        {
            _logger.LogInformation("making call");
            var key = _secretClient.GetSecretAsync("test").Result;
            _logger.LogInformation($"got secret for test:{key}");
        }
    }
}
