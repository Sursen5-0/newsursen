using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Severa
{
    public class RetryHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
    {
        private const int MAX_RETRIES = 2;
        private const int MESSAGE_DELAY_MS = 1000;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            for (int i = 0; i < MAX_RETRIES; i++)
            {
                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
                response = await base.SendAsync(request, cancellationToken);
                Thread.Sleep(MESSAGE_DELAY_MS);
                Console.WriteLine("Request failed trying again (" + (i + 1) + " of " + MAX_RETRIES + ")[" + response.StatusCode + "]");
            }
            Console.WriteLine("Request failed within maximum attempts of " + MAX_RETRIES);
            return response;
        }
    }
}
