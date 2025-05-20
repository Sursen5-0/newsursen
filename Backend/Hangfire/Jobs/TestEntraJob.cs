using Infrastructure.Entra;
using Microsoft.Extensions.Logging;

namespace Hangfire.Jobs
{
    public class TestEntraJob
    {
        private readonly EntraClient _entraClient;
        private readonly ILogger<TestEntraJob> _logger;

        public TestEntraJob(EntraClient entraClient, ILogger<TestEntraJob> logger)
        {
            _entraClient = entraClient;
            _logger = logger;
        }

        public async Task WriteTestToken()
        {
            try
            {
                var token = await _entraClient.GetTokenAsync();
                _logger.LogInformation("Received token: {Token}", token);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to retrieve Entra token: {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in TestEntraJob: {Message}", ex.Message);
            }
        }
    }
}