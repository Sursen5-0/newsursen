using Application.Secrets;
using Infrastructure.Secrets;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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

        public async Task<string> GetApiKey()
        {
            return _flowCaseSecret;
/*            if (string.IsNullOrEmpty(_flowCaseSecret))
            {
                throw new InvalidOperationException("FlowCase API key is not set or is empty.");
            }
            else
            {
              
                return _flowCaseSecret;
            }*/
        }

        public async Task<string> GetCV()
        {
            return "";
        }
    }
}