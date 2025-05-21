using Application.Secrets;
using Infrastructure.FlowCase;


namespace HangFire.Jobs
{


    public class FlowCaseJob
    {
        private readonly FlowCaseClient _flowCaseClient;
        private readonly ILogger<FlowCaseJob> _logger;

        public FlowCaseJob(FlowCaseClient flowCaseClient, ILogger<FlowCaseJob> logger)
        {
            _flowCaseClient = flowCaseClient;
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