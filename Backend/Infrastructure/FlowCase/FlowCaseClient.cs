using Domain.Interfaces.ExternalClients;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Infrastructure.FlowCase.Models;
using Infrastructure.Common;

namespace Infrastructure.FlowCase
{
    public class FlowCaseClient
    {
        private readonly string _flowCaseSecret;
        private readonly HttpClient _client;
        private const string FLOWCASE_KEY = "FLOWCASE_KEY";

        public FlowCaseClient(ISecretClient secretClient, HttpClient httpClient)
        {
            _flowCaseSecret = secretClient.GetSecretAsync(FLOWCASE_KEY).Result;
            _client = httpClient;
        }

        public async Task<FlowcaseReturnModel<FlowcaseUserModel>> GetUser(string email)
        {
            var uri = $"/api/v1/users/find?email={Uri.EscapeDataString(email)}";
            return await MakeRequest<FlowcaseUserModel>(uri);
        }


        private async Task<FlowcaseReturnModel<T>> MakeRequest<T>(string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _flowCaseSecret);

            var responseMessage = await _client.SendAsync(request);
            var apiResponse = await responseMessage.Content.ReadAsStringAsync();
            var returnModel = new FlowcaseReturnModel<T>();
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
                catch (JsonException ex)
                {
                    returnModel.IsSuccess = false;
                    returnModel.Message = $"Error deserializing response: {ex.Message}";
                }
                returnModel.StatusCode = responseMessage.StatusCode;

            }
                return returnModel;

        }
    }
}