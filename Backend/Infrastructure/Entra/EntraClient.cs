using Domain.Interfaces.ExternalClients;
using Domain.Models;
using Infrastructure.Common;
using Infrastructure.Entra.Mappers;
using Infrastructure.Entra.Models;
using Infrastructure.Persistance.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Entra
{
    public class EntraClient(
        ISecretClient SecretClient,
        HttpClient _httpClient,
        ILogger<RetryHandler> _logger
    ) : IEntraClient
    {
        private string? _token;

        public async Task<string?> GetTokenAsync()
        {
            if (!string.IsNullOrEmpty(_token))
                return _token;

            try
            {
                var tenantId = await SecretClient.GetSecretAsync(URLExtensions.TenantIdSecretName);
                var clientId = await SecretClient.GetSecretAsync(URLExtensions.ClientIdSecretName);
                var clientSecret = await SecretClient.GetSecretAsync(URLExtensions.ClientSecretName);

                var payload = URLExtensions.CreateTokenRequestPayload(clientId, clientSecret);
                var tokenUrl = URLExtensions.GetTokenEndpoint(tenantId);

                var req = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
                {
                    Content = new FormUrlEncodedContent(payload)
                };
                var res = await _httpClient.SendAsync(req);
                if (!res.IsSuccessStatusCode)
                {
                    var err = await res.Content.ReadAsStringAsync();
                    _logger.LogError("Token request failed with {StatusCode}: {Error}", res.StatusCode, err);
                    return null;
                }

                var json = await res.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);
                if (tokenResponse is null)
                {
                    _logger.LogError("Failed to deserialize token JSON: {Json}", json);
                    return null;
                }

                _token = tokenResponse.AccessToken;
                return _token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetTokenAsync");
                throw;
            }
        }

        public async Task<List<EmployeeDTO>> GetAllEmployeesAsync()
        {
            var accessToken = await GetTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("No access token available, aborting user retrieval");
                throw new InvalidOperationException("Failed to acquire Entra access token.");
            }

            var dtos = new List<EmployeeDTO>();
            var nextLink = URLExtensions.BuildUsersEndpoint();

            while (!string.IsNullOrEmpty(nextLink))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, nextLink);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Add("ConsistencyLevel", "eventual");

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Graph /users request failed with {StatusCode}: {Error}", response.StatusCode, errorContent);
                    throw new HttpRequestException($"Graph /users request failed with {response.StatusCode}: {errorContent}");
                }

                var page = await response.Content.ReadFromJsonAsync<GraphUsersPage<EntraEntityModel>>();
                if (page?.Value != null)
                {
                    foreach (var raw in page.Value)
                    {
                        dtos.Add(JsonToDtoEmployeeMapper.ToDto(raw));
                    }

                    _logger.LogInformation("Fetched and mapped {Count} users", page.Value.Count);
                }

                nextLink = page?.NextLink;
            }

            return dtos;
        }
    }
}