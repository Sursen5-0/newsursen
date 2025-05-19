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
        private static readonly int[] NonRetriableStatusCodes = { 400, 401, 403, 404 };


        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            for (int attempt = 0; attempt < MAX_RETRIES; attempt++)
            {
                var response = await base.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode || NonRetriableStatusCodes.Contains((int)response.StatusCode))
                {
                    return response;
                }
                _logger.LogWarning($"Request failed trying again ({attempt + 1} of {MAX_RETRIES})[{response.StatusCode}]");
                await Task.Delay(MESSAGE_DELAY_MS, cancellationToken);
            }
            _logger.LogError($"Request failed within maximum attempts of {MAX_RETRIES}");
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
