using Domain.Interfaces.ExternalClients;
using Domain.Models;
using Infrastructure.Severa.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using Infrastructure.Severa.Mappers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;

namespace Infrastructure.Severa
{
    public class SeveraClient : ISeveraClient
    {
        private readonly string _severaClientId;
        private readonly string _severaClientSecret;
        private static readonly string SCOPE = "users:read,hours:read";
        private static readonly string SEVERA_ROOT_URL = "https://api.severa.visma.com/rest-api/v1.0/";
        private static readonly string SEVERA_CLIENT_SECRET = "SEVERA_CLIENT_SECRET";
        private static readonly string SEVERA_CLIENT_ID = "SEVERA_CLIENT_ID";
        private readonly HttpClient _client;
        private readonly ILogger<SeveraClient> _logger;
        private string? _token = null;

        public SeveraClient(ISecretClient secretClient, HttpClient httpClient, ILogger<SeveraClient> logger)
        {
            _severaClientId = secretClient.GetSecretAsync(SEVERA_CLIENT_ID).Result;
            _severaClientSecret = secretClient.GetSecretAsync(SEVERA_CLIENT_SECRET).Result;
            _client = httpClient;
            _logger = logger;
        }
        public async Task<string> GetToken()
        {
            if (_token != null)
                return _token;
            var tokenBody = new TokenBody()
            {
                ClientId = _severaClientId,
                ClientSecret = _severaClientSecret,
                Scope = SCOPE
            };

            var response = await _client.PostAsJsonAsync(SEVERA_ROOT_URL + "token", tokenBody);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Severa client was unable to get token, returned HTTP {response.StatusCode}");
            }
            var jsonResponseBody = await response.Content.ReadAsStringAsync();
            var responseBody = JsonSerializer.Deserialize<TokenReturnModel>(jsonResponseBody);
            if (responseBody == null)
            {
                throw new SerializationException($"Unable to convert tokenresponse to expected format");
            }
            _token = responseBody.AccessToken;
            return _token;
        }
        public async Task<EmployeeContractDTO> GetWorkContractByUserId(Guid userId)
        {
            var response = await GetEntity<SeveraWorkContract>($"users/{userId}/workcontracts/current");
            if (!response.IsSuccess)
            {
                var message = $"Severa client was unable to get workcontract, returned HTTP {response.StatusCode}";
                _logger.LogError(message);
                throw new HttpRequestException(message);
            }
            else if (response.Data == null)
            {
                var message = $"Severa was able to get workcontract, but returned empty response";
                _logger.LogError(message);
                throw new HttpRequestException(message);
            }
            var contract = response.Data.ToDto(userId);
            return contract;
        }
        public async Task<SeveraEmployeeModel?> GetUserByEmail(string email)
        {
            var list = new List<SeveraEmployeeModel>();
            var response = await GetEntity<List<SeveraEmployeeModel>>($"users?email={email}");
            if (!response.IsSuccess && response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogError($"Unable to find user with email: {email} in severa");
                return null;
            }
            else if (!response.IsSuccess)
            {
                _logger.LogError($"Severa returned {response.Message} in get user by email");
                return null;
            }
            else if (response.Data == null)
            {
                _logger.LogError($"Severa returned no error for call for user, but didnt return any data");
                return null;
            }
            else if (response.Data.Count > 1)
            {
                _logger.LogError($"Severa returned more than one user for the email. Returning the first");
            }
            return response.Data.FirstOrDefault();
        }
        private async Task<SeveraReturnModel<T>> GetEntity<T>(string path)
        {
            var pathCombined = SEVERA_ROOT_URL + path;
            var request = new HttpRequestMessage(HttpMethod.Get, pathCombined);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetToken());
            var responseMessage = await _client.SendAsync(request); var returnModel = new SeveraReturnModel<T>();
            var apiResponse = await responseMessage.Content.ReadAsStringAsync();
            returnModel.IsSuccess = responseMessage.IsSuccessStatusCode;
            if (!responseMessage.IsSuccessStatusCode)
            {
                returnModel.Message = apiResponse;
            }
            else
            {
                try
                {
                    returnModel.Data = JsonSerializer.Deserialize<T>(apiResponse);
                }
                catch (Exception)
                {
                    _logger.LogError(@$"Unable to deserialize the following data into into {typeof(T).Name}:
                    {apiResponse}");
                    throw new SerializationException();
                }
            }
            returnModel.StatusCode = responseMessage.StatusCode;
            return returnModel;
        }


    }
}