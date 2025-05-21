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
            var response = await GetEntityByID<SeveraWorkContract>($"users/{userId}/workcontracts/current");
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

        private async Task<SeveraReturnModel<T>> GetEntityByID<T>(string path)
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