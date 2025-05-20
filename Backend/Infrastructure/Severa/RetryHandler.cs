using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Severa
{
    public class RetryHandler(HttpMessageHandler _innerHandler, ILogger<RetryHandler> _logger) : DelegatingHandler(_innerHandler)
    {
        private const int MAX_RETRIES = 3;
        private const int MESSAGE_DELAY_MS = 1000;
        private static readonly int[] NonRetriableStatusCodes = [400, 401, 403, 404];


        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null!;

            for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
            {
                response = await base.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode || NonRetriableStatusCodes.Contains((int)response.StatusCode))
                {
                    return response;
                }
                _logger.LogWarning($"Request failed trying again ({attempt + 1} of {MAX_RETRIES})[{response.StatusCode}]");
                var jitter = Random.Shared.Next(100, 300);
                await Task.Delay((attempt * MESSAGE_DELAY_MS) + jitter, cancellationToken);
            }
            _logger.LogError($"Request failed within maximum attempts of {MAX_RETRIES}");
            return response;
        }
    }
}
