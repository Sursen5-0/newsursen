using Application.Secrets;
using Infrastructure.Severa;
using Infrastructure.Entra;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Hangfire.Jobs
{
    public class TestJob
    {
        private readonly EntraClient _severaClient;
        private readonly ILogger<TestJob> _logger;

        public TestJob(EntraClient severaClient, ILogger<TestJob> logger)
        {
            _severaClient = severaClient;
            _logger = logger;
        }

        public async Task WriteTest()
        {
            _logger.LogInformation("making call");

            // Call the client to get the users JSON
            var json = await _severaClient.GetUsersJsonAsync();

            _logger.LogInformation($"got users JSON: {json}");

            // Write the raw JSON to the Visual Studio Output (Debug) window
            Debug.WriteLine("Users JSON:");
            Debug.WriteLine(json);
        }
    }
}
