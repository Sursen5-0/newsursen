using Domain.Interfaces.ExternalClients;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;

namespace Infrastructure.Secrets
{
    public class DopplerClient : ISecretClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _environment;
        private readonly string _project = "sursen";
        public DopplerClient(HttpClient httpClient,string doppler_key, string environment)
        {
            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(doppler_key);
            ArgumentNullException.ThrowIfNull(environment);

            _environment = environment;
            _httpClient = httpClient;

            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", doppler_key);
        }
        public async Task<string> GetSecretAsync(string key, CancellationToken? token = null)
        {
            var name = key.ToUpper();
            var url = $"configs/config/secret?project={_project}&config={_environment}&name={name}";

            var response = await _httpClient.GetAsync(url, token.GetValueOrDefault());
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
