using System.Diagnostics;
using System.Threading.Tasks;
using Domain.Interfaces.ExternalClients;
using Microsoft.Extensions.Logging;

namespace Hangfire.Jobs
{
    public class TestJob
    {
        private readonly IEntraClient _entraClient;
        private readonly ILogger<TestJob> _logger;

        public TestJob(IEntraClient entraClient, ILogger<TestJob> logger)
        {
            _entraClient = entraClient;
            _logger = logger;
        }

        public async Task WriteTest()
        {
            _logger.LogInformation("making call");

            var json = await _entraClient.GetUsersJsonAsync();

            _logger.LogInformation("got users JSON: {json}", json);

            Debug.WriteLine("Users JSON:");
            Debug.WriteLine(json);
        }
    }
}
