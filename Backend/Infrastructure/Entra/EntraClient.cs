// EntraClient.cs
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Secrets;
using Infrastructure.Entra.Models;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Entra
{
    public class EntraClient
    {
        private readonly ISecretClient _secretClient;
        private readonly ILogger<EntraRetryHandler> _retryLogger;
        private string? _token;

        public EntraClient(
            ISecretClient secretClient,
            ILogger<EntraRetryHandler> retryLogger)
        {
            _secretClient = secretClient;
            _retryLogger = retryLogger;
        }

        public async Task<string?> GetTokenAsync()
        {
            if (!string.IsNullOrEmpty(_token))
                return _token;

            try
            {
                // 1) retrieve vault secrets
                var tenantId = await _secretClient.GetSecretAsync(URLExtensions.TenantIdSecretName);
                var clientId = await _secretClient.GetSecretAsync(URLExtensions.ClientIdSecretName);
                var clientSecret = await _secretClient.GetSecretAsync(URLExtensions.ClientSecretName);

                // 2) build payload dict & endpoint URL
                Dictionary<string, string> payload =
                    URLExtensions.CreateTokenRequestPayload(clientId, clientSecret);
                var tokenUrl = URLExtensions.GetTokenEndpoint(tenantId);

                // 3) send request
                var handler = new EntraRetryHandler(_retryLogger);
                using var http = new HttpClient(handler);
                using var req = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
                {
                    Content = new FormUrlEncodedContent(payload)
                };

                using var res = await http.SendAsync(req);
                if (!res.IsSuccessStatusCode)
                {
                    var err = await res.Content.ReadAsStringAsync();
                    _retryLogger.LogError(
                        "Token request failed with {StatusCode}: {Error}",
                        res.StatusCode, err);
                    return null;
                }

                var json = await res.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);
                if (tokenResponse is null)
                {
                    _retryLogger.LogError("Failed to deserialize token JSON: {Json}", json);
                    return null;
                }

                _token = tokenResponse.AccessToken;
                return _token;
            }
            catch (HttpRequestException ex)
            {
                _retryLogger.LogError(ex, "HTTP error fetching Entra token");
            }
            catch (JsonException ex)
            {
                _retryLogger.LogError(ex, "JSON error parsing Entra token response");
            }
            catch (Exception ex)
            {
                _retryLogger.LogError(ex, "Unexpected error in GetTokenAsync");
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
                    _retryLogger.LogError(
                        "No access token available, aborting GetUsersJsonAsync");
                    return null;
                }

                var handler = new EntraRetryHandler(_retryLogger);
                using var http = new HttpClient(handler);

                // build the /users URL dynamically
                var usersUrl = URLExtensions.BuildUsersEndpoint();
                using var req = new HttpRequestMessage(HttpMethod.Get, usersUrl);

                req.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                req.Headers.Add("ConsistencyLevel", "eventual");

                using var res = await http.SendAsync(req);
                if (!res.IsSuccessStatusCode)
                {
                    var err = await res.Content.ReadAsStringAsync();
                    _retryLogger.LogError(
                        "Graph /users request failed with {StatusCode}: {Error}",
                        res.StatusCode, err);
                    return null;
                }

                return await res.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                _retryLogger.LogError(ex, "HTTP error fetching Graph users");
            }
            catch (JsonException ex)
            {
                _retryLogger.LogError(ex, "JSON error parsing Graph users response");
            }
            catch (Exception ex)
            {
                _retryLogger.LogError(ex, "Unexpected error in GetUsersJsonAsync");
            }

            return null;
        }
    }
}
