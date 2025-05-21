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

namespace Infrastructure.FlowCase
{
    public class FlowCaseClient
    {
        private readonly ISecretClient _secretClient;
        private readonly HttpClient _client;
        private const string FLOWCASE_KEY = "FLOWCASE_KEY";

        public FlowCaseClient(ISecretClient secretClient, HttpClient httpClient)
        {
            _secretClient = secretClient;
            _client = httpClient;
        }

        public Task<string> GetApiKey()
        {
            // Always fetch the latest key asynchronously
            return _secretClient.GetSecretAsync(FLOWCASE_KEY);
        }

        public async Task<string> GetCV()
        {
            return "";
        }
    }
}