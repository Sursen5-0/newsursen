using Application.Secrets;
using Infrastructure.Severa.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Severa
{
    public class SeveraClient
    {
        private readonly string _severaClientId;
        private readonly string _severaClientSecret;
        private static readonly string SCOPE = "users:read,hours:read";
        private static readonly string SEVERA_ROOT_URL = "https://api.severa.visma.com/rest-api/v1.0";
        private static readonly string SEVERA_CLIENT_SECRET = "SEVERA_CLIENT_SECRET";
        private static readonly string SEVERA_CLIENT_ID = "SEVERA_CLIENT_ID";
        private readonly HttpClient _client;
        private string? _token;
        public string Token
        {
            get
            {
                _token ??= GetToken().Result;
                return _token;
            }
        }

        public SeveraClient(ISecretClient secretClient, HttpClient httpClient)
        {
            _severaClientId = secretClient.GetSecretAsync(SEVERA_CLIENT_ID).Result;
            _severaClientSecret = secretClient.GetSecretAsync(SEVERA_CLIENT_SECRET).Result;
            _client = httpClient;
        }
        public string GetTokenTest()
        {
            return GetToken().Result;
        }
        private async Task<string> GetToken()
        {
            var tokenBody = new TokenBody()
            {
                ClientId = _severaClientId,
                ClientSecret = _severaClientSecret,
                Scope = SCOPE
            };

            var response = await _client.PostAsJsonAsync(SEVERA_ROOT_URL + "/token", tokenBody);
            if(!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Severa client was unable to get token, returned HTTP {response.StatusCode}");
            }
            var jsonResponseBody = await response.Content.ReadAsStringAsync();
            var responseBody = JsonSerializer.Deserialize<TokenReturnModel>(jsonResponseBody);
            if(responseBody == null)
            {
                throw new SerializationException($"Unable to convert tokenresponse to expected format");
            }
            return responseBody.AccessToken;
        }
    }
}
