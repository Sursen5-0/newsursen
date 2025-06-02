using Domain.Interfaces.ExternalClients;
using Domain.Models;
using Infrastructure.Common;
using Infrastructure.FlowCase.Models;
using Infrastructure.Severa;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
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

namespace Infrastructure.FlowCase
{
    public class FlowCaseClient : IFlowCaseClient
    {
        private readonly string _flowCaseSecret;
        private readonly HttpClient _client;
        private const string FLOWCASE_KEY = "FLOWCASE_KEY";
        private readonly ILogger<FlowCaseClient> _logger;
        private readonly string[] _officeNames ={"Minds"}; // Add more office names if needed

        public FlowCaseClient(ISecretClient secretClient, HttpClient httpClient, ILogger<FlowCaseClient> logger)
        {
            _flowCaseSecret = secretClient.GetSecretAsync(FLOWCASE_KEY).Result;
            _client = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a list of skills from a CV for a specified user and CV ID.
        /// </summary>
        /// <remarks>This method retrieves skills from a CV by making an API call to the specified
        /// endpoint.  Skills with empty or whitespace names are skipped, and a warning is logged for each skipped
        /// skill.</remarks>
        /// <param name="user_id">The unique identifier of the user whose CV is being accessed. Cannot be null or empty.</param>
        /// <param name="cv_id">The unique identifier of the CV from which skills are to be retrieved. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="SkillDTO"/>
        /// objects representing the skills extracted from the CV.</returns>
        /// <exception cref="Exception">Thrown if the operation to retrieve skills fails, including details of the failure in the exception message.</exception>
        public async Task<List<SkillDTO>> GetSkillsFromCVAsync(string userId, string cvId)
        {
            List<SkillDTO> skills = new List<SkillDTO>();
            var uri = $"api/v3/cvs/{userId}/{cvId}";
            var response = await MakeRequest<List<FlowcaseSkillModel>>(uri);
            if (!response.IsSuccess)
            {
                throw new Exception($"Failed to retrieve skills: {response.Message}");
            }

            foreach (var skill in response.Data)
            {
                if (string.IsNullOrWhiteSpace(skill.Values.Name))
                {
                    _logger.LogWarning($"Skill with ID {skill.SkillId} has an empty name and will be skipped.");
                    continue;
                }
                var skillDto = new SkillDTO
                {
                    Id = Guid.NewGuid(),
                    SkillId = skill.SkillId,
                    SkillName = skill.Values.Name,
                    SkillTotalDurationInYears = skill.TotalDurationInYears,
                };
                skills.Add(skillDto);
            }

            return skills;
        }

        /// <summary>
        /// Retrieves a list of skills from the Flowcase API and inserts them into the SKills table.
        /// </summary>
        /// <remarks>This method fetches skills in batches from the Flowcase API, processes them into a
        /// list of <see cref="SkillDTO"/> objects, and returns the aggregated result. Skills with empty or null names
        /// are skipped, and a warning is logged for each skipped skill.</remarks>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The result contains a list of <see
        /// cref="SkillDTO"/> objects representing the skills retrieved from the Flowcase API.</returns>
        /// <exception cref="Exception">Thrown if the API request fails or returns an unsuccessful response.</exception>
        public async Task<List<SkillDTO>> GetSkillsFromFlowcaseAsync()
        {

            List<SkillDTO> skills = new List<SkillDTO>();
            int offset = 0;
            int limit = 100;

            var uri = $"api/v1/masterdata/technologies/tags?offset={offset}&limit={limit}";
            var response = await MakeRequest<List<FlowcaseSkillModel>>(uri);
            if (!response.IsSuccess)
            {
                throw new Exception($"Failed to retrieve skills: {response.Message}");
            }

            while (response.Data != null && response.Data.Count > 0)
            {
                foreach (var skill in response.Data)
                {
                    if (string.IsNullOrWhiteSpace(skill.Values.Name))
                    {
                        _logger.LogWarning($"Skill with ID {skill.SkillId} has an empty name and will be skipped.");
                        continue;
                    }

                    var skillDto = new SkillDTO
                    {
                        Id = Guid.NewGuid(),
                        SkillId = skill.SkillId,
                        SkillName = skill.Values.Name,
                    };
                    skills.Add(skillDto);
                }

                offset += limit;
                uri = $"api/v1/masterdata/technologies/tags?offset={offset}&limit={limit}";
                response = await MakeRequest<List<FlowcaseSkillModel>>(uri);
            }

            return skills;
        }
        
        public async Task<List<FlowcaseUserModel>> GetUsersAsync()
        {
            List<FlowcaseUserModel> users = new();
            int offset = 0;
            int limit = 500;
            var uri = $"api/v2/users/search?from={offset}&size={limit}&sort_by=country&deactivated=false";
            var response = await GetEntity<List<FlowcaseUserModel>>(uri);
            if (!response.IsSuccess)
            {
                throw new Exception($"Failed to retrieve users: {response.Message}");
            }
            while (response.Data != null && response.Data.Count > 0)
            {
                foreach (var user in response.Data)
                {
                    if (!_officeNames.Any(o => user.OfficeName.Equals(o, StringComparison.OrdinalIgnoreCase))) // Skip users not in Minds office
                    {
                        _logger.LogWarning($"User with ID {user.UserId} is not in Minds office and will be skipped.");
                        continue;
                    }
                    users.Add(user);
                }
                offset += limit;
                uri = $"api/v1/users?offset={offset}&limit={limit}";
                response = await MakeRequest<List<FlowcaseUserModel>>(uri);
            }
            return users;

        }



        public async Task<FlowcaseReturnModel<FlowcaseUserModel>> GetUser(string email)
        {
            var uri = $"/api/v1/users/find?email={Uri.EscapeDataString(email)}";
            return await MakeRequest<FlowcaseUserModel>(uri);
        }

        private async Task<FlowcaseReturnModel<T>> GetEntity<T>(string path)
        {
            return await MakeRequest<T>(path);
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