using Application.Secrets;
using Infrastructure.FlowCase;


namespace HangFire.Jobs
{


    public class FlowCaseJob
    {
        private readonly FlowCaseClient _flowCaseClient;
        private readonly ILogger<FlowCaseJob> _logger;

        public FlowCaseJob(SeveraClient severaClient, ILogger<FlowCaseJob> logger)
        {
            _severaClient = severaClient;
            _logger = logger;
        }

        public void WriteTest()
        {
            _logger.LogInformation("Fetching CV");
            var key = _flowCaseClient.GetCV().Result;
            _logger.LogInformation($"got secret for test:{key}");
        }
    }
}