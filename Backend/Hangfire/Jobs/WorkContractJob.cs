using Infrastructure.Severa;
using Infrastructure.Severa.Models;

namespace Hangfire.Jobs
{
    public class WorkContractJob(SeveraClient severaClient, ILogger<WorkContractJob> _logger)
    {
        public void Run()
        {
            var item = severaClient.GetWorkContractByUserId(Guid.Parse("3189d3d2-4671-b3c4-fbb8-6aa4cfc0933a")).Result;
            _logger.LogInformation(item.Data.First().Title);

        }
    }
}
