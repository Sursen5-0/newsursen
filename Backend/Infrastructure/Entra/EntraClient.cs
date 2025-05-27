using Domain.Interfaces.ExternalClients;
using Domain.Models;
using Infrastructure.Common;
using Infrastructure.Entra.Models;
using Infrastructure.Persistance.Mappers;
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
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error fetching Entra token");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON error parsing Entra token response");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetTokenAsync");
            }

            return null;
        }

        public async Task<List<EmployeeDTO>> GetAllUsersAsync()
        {
            var accessToken = await GetTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("No access token available, aborting user retrieval");
                return new List<EmployeeDTO>();
            }

            var dtos = new List<EmployeeDTO>();
            var nextLink = URLExtensions.BuildUsersEndpoint();

            while (!string.IsNullOrEmpty(nextLink))
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, nextLink);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Add("ConsistencyLevel", "eventual");

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to fetch users page: {StatusCode} - {Error}", response.StatusCode, error);
                    break;
                }

                // Deserialize raw JSON into EntraEmployeePage
                var page = await response.Content.ReadFromJsonAsync<EntraEmployeePage<EntraEmployeeModel>>();
                if (page?.Value != null)
                {
                    // Map each raw model to EmployeeDTO via JsonToDtoEmployeeMapper
                    foreach (var raw in page.Value)
                    {
                        var dto = JsonToDtoEmployeeMapper.ToDto(raw);
                        dtos.Add(dto);
                    }

                    _logger.LogInformation("Fetched and mapped {Count} users", page.Value.Count);
                }

                nextLink = page?.NextLink;
            }

            return dtos;
        }
    }
}
