using Application.Secrets;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;

namespace Infrastructure.Secrets
{
    public class DopplerClient : ISecretClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DopplerClient> _logger;
        public DopplerClient(HttpClient httpClient, ILogger<DopplerClient> logger)
        {
            _logger = logger;
            var token = Environment.GetEnvironmentVariable("doppler_key", EnvironmentVariableTarget.Machine);
            if(token == null)
            {
                _logger.LogError("Environment variable: doppler_key is not set.");
                throw new Exception($"Environment variable: doppler_key is not set.");
            }

            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.doppler.com/v3/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        public async Task<string> GetSecretAsync(string key, CancellationToken? token = null)
        {
            var project = "sursen";
            var config = "dev";
            var name = key.ToUpper();
            var url = $"configs/config/secret?project={project}&config={config}&name={name}";

            var response = await _httpClient.GetAsync(url, token ?? new CancellationToken());
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var result = await response.Content.ReadFromJsonAsync<SecretResponse>();
                    return result!.Secret.Value;
                case System.Net.HttpStatusCode.Unauthorized:
                case System.Net.HttpStatusCode.Forbidden:
                    throw new Exception("SecretClient is not authorized to view secret");
                case System.Net.HttpStatusCode.NotFound:
                    throw new Exception("Secret is not found");
                case System.Net.HttpStatusCode.InternalServerError:
                case System.Net.HttpStatusCode.BadGateway:
                case System.Net.HttpStatusCode.ServiceUnavailable:
                case System.Net.HttpStatusCode.GatewayTimeout:
                    throw new Exception("Doppler service is temporarily unavailable");
                default:
                    throw new Exception("Undhandled exception in SecretClient response");
            }
        }
    }
}
