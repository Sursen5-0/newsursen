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
        private readonly string _flowCaseClientId;
        private readonly string _flowCaseClientSecret;

        private static readonly string FLOWCASE_ROOT_URL = "https://twoday.flowcase.com/api/v3/cvs"; //<---- grund URL Flowcase API for CV's
        private static readonly string FLOWCASE_KEY = "FLOWCASE_KEY"; //<---- Key for Flowcase API

        private readonly HttpClient _client;
        private string? _token = null;

        public FlowCaseClient(ISecretClient secretClient, HttpClient httpClient)
        {
            _flowCasekey = secretClient.GetSecretAsync(FLOWCASE_KEY).Result;
            _client = httpClient;
        }
    }