using Application.Secrets;
using Infrastructure.Entra.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Entra
{
    public class EntraClient
    {
        private readonly ISecretClient _secretClient;
        private readonly HttpClient _httpClient;
        private string? _token;

        private const string TokenEndpointTemplate = "https://login.microsoftonline.com/{0}/oauth2/v2.0/token";
        private const string Scope = "https://graph.microsoft.com/.default";
        private const string ClientIdSecretName = "ENTRA_ID";
        private const string ClientSecretName = "ENTRA_SECRET";
        private const string TenantIdSecretName = "ENTRA_TENANT";

        public EntraClient(ISecretClient secretClient, HttpClient httpClient)
        {
            _secretClient = secretClient;
            _httpClient = httpClient;
        }

        public async Task<string> GetTokenAsync()
        {
            if (!string.IsNullOrEmpty(_token))
                return _token;

            // Retrieve credentials and tenant dynamically from Doppler
            var tenantId = await _secretClient.GetSecretAsync(TenantIdSecretName);
            var clientId = await _secretClient.GetSecretAsync(ClientIdSecretName);
            var clientSecret = await _secretClient.GetSecretAsync(ClientSecretName);

            // Write values to Visual Studio Output window
            Debug.WriteLine($"[EntraClient] Tenant: {tenantId}");
            Debug.WriteLine($"[EntraClient] ClientId: {clientId}");
            Debug.WriteLine($"[EntraClient] ClientSecret length: {clientSecret.Length}");

            var parameters = new Dictionary<string, string>
            {
                {"client_id", clientId},
                {"client_secret", clientSecret},
                {"scope", Scope},
                {"grant_type", "client_credentials"}
            };

            var requestUri = string.Format(TokenEndpointTemplate, tenantId);
            Debug.WriteLine($"[EntraClient] Requesting token from: {requestUri}");

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = new FormUrlEncodedContent(parameters)
            };

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[EntraClient] Token endpoint returned {response.StatusCode}: {errorContent}");
                response.EnsureSuccessStatusCode();
            }

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content)
                ?? throw new InvalidOperationException("Unable to deserialize token response.");

            _token = tokenResponse.AccessToken;
            return _token;
        }
    }
}