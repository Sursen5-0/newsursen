using Domain.Interfaces.ExternalClients;
using Domain.Models;
using Infrastructure.Severa.Models;
using Microsoft.Extensions.Logging;
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
using System.Xml;
using System.Collections;

namespace Infrastructure.Severa
{
    public class SeveraClient : ISeveraClient
    {
        private readonly string _severaClientId;
        private readonly string _severaClientSecret;
        private static readonly string SCOPE = "users:read,hours:read,activities:read";
        private static readonly string SEVERA_CLIENT_SECRET = "SEVERA_CLIENT_SECRET";
        private static readonly string SEVERA_CLIENT_ID = "SEVERA_CLIENT_ID";
        private static readonly string NEXT_PAGE_TOKEN = "NextPageToken";
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

            var response = await _client.PostAsJsonAsync("token", tokenBody);
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
        public async Task<IEnumerable<AbsenceDTO>?> GetAbsence()
        {
            var response = await GetEntities<SeveraActivityModel>($"activities?&activityCategories=Absences&userGuids=0162a796-5739-d14b-0ad7-a01e3c4e762c");

            if (!response.IsSuccess)
            {
                _logger.LogError($"Severa returned {response.Message} in abscence");
                return null;
            }
            else if (response.Data == null)
            {
                _logger.LogError($"Severa returned no error for call for user, but didnt return any data");
                return null;
            }
            response.Data = response.Data.Where(x => Guid.TryParse(x.Identifier, out _));
            return response.Data.Select(x => x.ToDto());

        }

        private async Task<SeveraReturnModel<T>> GetEntity<T>(string path)
        {

            return await MakeRequest<T>(path);
        }
        private async Task<SeveraReturnModel<IEnumerable<T>>> GetEntities<T>(string path)
        {
            var model = new SeveraReturnModel<IEnumerable<T>>();
            var list = new List<T>();
            string? nextToken = null;
            var moreData = true;
            do
            {
                var result = await MakeRequest<List<T>>(path, nextToken);
                if (result != null && result.Data != null)
                {
                    list.AddRange(result.Data);
                }
                moreData = result != null && result.IsSuccess && !string.IsNullOrWhiteSpace(result.NextToken);
                model.IsSuccess = result?.IsSuccess ?? false;
                nextToken = result?.NextToken;
            }
            while (moreData);
            model.Data = list;
            return model;

        }
        private async Task<SeveraReturnModel<T>> MakeRequest<T>(string path, string? nextToken = null)
        {
            if (nextToken != null)
            {
                path = path + "&pageToken=" + nextToken;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetToken());
            var responseMessage = await _client.SendAsync(request);
            var returnModel = new SeveraReturnModel<T>();
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
                    if (responseMessage.Headers.Contains(NEXT_PAGE_TOKEN))
                    {
                        responseMessage.Headers.TryGetValues(NEXT_PAGE_TOKEN, out var values);
                        if (values != null && values.Count() != 0)
                        {

                            returnModel.NextToken = values.First();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unable to deserialize the following data into into {Name}:{apiResponse}", typeof(T).Name, apiResponse);
                    throw new SerializationException(ex.Message);
                }
            }
            returnModel.StatusCode = responseMessage.StatusCode;
            return returnModel;
        }
    }
}