using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Entra
{
    public class EntraRetryHandler(HttpMessageHandler _innerHandler, ILogger<EntraRetryHandler> _logger) : DelegatingHandler(_innerHandler)
    {
        private const int MAX_RETRIES = 3;
        private const int MESSAGE_DELAY_MS = 1000;
        private static readonly int[] NonRetriableStatusCodes = { 400, 401, 403, 404 };


        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
            {
                HttpResponseMessage response = null;
                try
                {
                    response = await base.SendAsync(request, cancellationToken);

                    if (response.IsSuccessStatusCode ||
                        NonRetriableStatusCodes.Contains((int)response.StatusCode))
                    {
                        return response;
                    }

                    _logger.LogWarning(
                        "Attempt {Attempt}/{Max} failed for {Url} with {Status}",
                        attempt, MAX_RETRIES, request.RequestUri, response.StatusCode);
                }
                catch (Exception ex) when (attempt < MAX_RETRIES)
                {
                    _logger.LogWarning(
                        ex, "Exception on attempt {Attempt}/{Max} for {Url}",
                        attempt, MAX_RETRIES, request.RequestUri);
                }

                await Task.Delay(MESSAGE_DELAY_MS, cancellationToken);
            }

            _logger.LogError("All {Max} retries exhausted for {Url}", MAX_RETRIES, request.RequestUri);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
