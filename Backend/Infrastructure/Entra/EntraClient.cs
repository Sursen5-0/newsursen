
using Application.Secrets;
using Infrastructure.Entra.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Entra
{
    public class EntraClient
    {
        private readonly ISecretClient _secretClient;
        private readonly ILogger<EntraRetryHandler> _retryLogger;
        private string? _token;

        private const string TokenEndpointTemplate =
            "https://login.microsoftonline.com/{0}/oauth2/v2.0/token";
        private const string Scope = "https://graph.microsoft.com/.default";
        private const string ClientIdSecretName = "ENTRA_ID";
        private const string ClientSecretName = "ENTRA_SECRET";
        private const string TenantIdSecretName = "ENTRA_TENANT";
        private const string UsersEndpoint =
            "https://graph.microsoft.com/v1.0/users" +
            "?$select=id,accountEnabled,ageGroup,businessPhones,city,companyName,country," +
            "createdDateTime,creationType,department,displayName,employeeHireDate,employeeId," +
            "employeeLeaveDateTime,employeeType,givenName,jobTitle,mail,mobilePhone,postalCode," +
            "surname,userPrincipalName,userType" +
            "&$filter=accountEnabled eq true and endswith(mail, '@twoday.com') and jobTitle ne null" +
            "&$count=true";

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
                var tenantId = await _secretClient.GetSecretAsync(TenantIdSecretName);
                var clientId = await _secretClient.GetSecretAsync(ClientIdSecretName);
                var clientSecret = await _secretClient.GetSecretAsync(ClientSecretName);

                var payload = new Dictionary<string, string>
                {
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["scope"] = Scope,
                    ["grant_type"] = "client_credentials"
                };

                var handler = new EntraRetryHandler(_retryLogger);
                using var http = new HttpClient(handler);
                var tokenUrl = string.Format(TokenEndpointTemplate, tenantId);

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
                if (tokenResponse == null)
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
                    _retryLogger.LogWarning("No access token available, aborting GetUsersJsonAsync");
                    return null;
                }

                var handler = new EntraRetryHandler(_retryLogger);
                using var http = new HttpClient(handler);
                using var req = new HttpRequestMessage(HttpMethod.Get, UsersEndpoint);

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
