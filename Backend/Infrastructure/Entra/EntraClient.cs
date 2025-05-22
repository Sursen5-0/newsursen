using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Interfaces.ExternalClients;
using Infrastructure.Entra.Models;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Entra
{
    public class EntraClient(
        ISecretClient SecretClient,
        HttpClient HttpClient,
        ILogger<EntraRetryHandler> _logger
    )
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
                var res = await HttpClient.SendAsync(req);

                if (!res.IsSuccessStatusCode)
                {
                    var err = await res.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Token request failed with {StatusCode}: {Error}",
                        res.StatusCode, err);
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

        public async Task<string?> GetUsersJsonAsync()
        {
            try
            {
                var accessToken = await GetTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogError(
                        "No access token available, aborting GetUsersJsonAsync");
                    return null;
                }

                var usersUrl = URLExtensions.BuildUsersEndpoint();
                var req = new HttpRequestMessage(HttpMethod.Get, usersUrl);

                req.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
                req.Headers.Add("ConsistencyLevel", "eventual");

                var res = await HttpClient.SendAsync(req);
                if (!res.IsSuccessStatusCode)
                {
                    var err = await res.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Graph /users request failed with {StatusCode}: {Error}",
                        res.StatusCode, err);
                    return null;
                }

                return await res.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error fetching Graph users");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON error parsing Graph users response");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetUsersJsonAsync");
            }

            return null;
        }
    }
}
