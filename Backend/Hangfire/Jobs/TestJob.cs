// TestJob.cs
using Infrastructure.Entra;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Hangfire.Jobs
{
    public record TestJob(EntraClient severaClient, ILogger<TestJob> _logger)
    {
        public async Task WriteTest()
        {
            _logger.LogInformation("making call");

            // Call the client to get the users JSON
            var json = await severaClient.GetUsersJsonAsync();

            _logger.LogInformation($"got users JSON: {json}");

            // Debug output
            Debug.WriteLine("Users JSON:");
            Debug.WriteLine(json);
        }
    }
}
