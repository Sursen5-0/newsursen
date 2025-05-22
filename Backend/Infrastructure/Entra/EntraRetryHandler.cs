// Infrastructure/Entra/EntraRetryHandler.cs
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Entra
{
    public class EntraRetryHandler : DelegatingHandler
    {
        private const int MAX_RETRIES = 3;
        private const int MESSAGE_DELAY_MS = 1000;
        private static readonly int[] NonRetriableStatusCodes = new[] { 400, 401, 403, 404 };

        private readonly ILogger<EntraRetryHandler> _logger;

        public EntraRetryHandler(HttpMessageHandler innerHandler,
                                 ILogger<EntraRetryHandler> logger)
            : base(innerHandler)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null!;

            for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
            {
                response = await base.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode ||
                    NonRetriableStatusCodes.Contains((int)response.StatusCode))
                {
                    return response;
                }

                _logger.LogWarning(
                    "Attempt {Attempt}/{Max} failed for {Url} with {Status}. Retrying…",
                    attempt, MAX_RETRIES, request.RequestUri, response.StatusCode);

                var jitter = Random.Shared.Next(100, 300);
                await Task.Delay((attempt * MESSAGE_DELAY_MS) + jitter, cancellationToken);
            }

            _logger.LogError(
                "All {Max} retries exhausted for {Url}",
                MAX_RETRIES, request.RequestUri);

            return response;
        }
    }
}
