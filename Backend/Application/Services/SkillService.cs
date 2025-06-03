using Domain.Interfaces.ExternalClients;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Application.Services
{
    public class SkillService(IFlowCaseClient _flowcaseClient, ISkillRepository _skillRepository, ILogger<SkillService> _logger) : ISkillService
    {
        /// <summary>
        /// Adds or updates skills in the SKills table based on the skills retrieved from Flowcase.
        /// </summary>
        /// <returns></returns>
        public async Task SynchronizeSkillsFromFlowcaseAsync()
        {
            _logger.LogInformation("Synchronizing skills from Flowcase");

            List<SkillDTO> skills = await _flowcaseClient.GetSkillsFromFlowcaseAsync();
            if (skills == null || !skills.Any())
            {
                _logger.LogWarning("No skills found in Flowcase, skipping synchronization.");
                return;
            }

            var existingSkills = await _skillRepository.GetAllSkillsAsync();
            var existingSkillId = existingSkills.Select(s => s.SkillId).ToHashSet();

            var newSkills = new List<SkillDTO>();
            foreach (var skill in skills)
            {
                if (!existingSkillId.Contains(skill.SkillId))
                {
                    _logger.LogInformation($"Adding new skill: {skill.SkillName}");
                    newSkills.Add(skill);
                }
                else
                {
                    _logger.LogInformation($"Skill already exists: {skill.SkillName}, skipping addition.");
                }
            }
            if (newSkills.Any())
            {
                await _skillRepository.AddSkillAsync(newSkills);
            }
        }

        public async Task SynchronizeSkillsFromCVAsync(string userId, string cvId)
        {

            _logger.LogInformation($"Retrieving skills from CV for user {userId} and CV {cvId}");
            await _flowcaseClient.GetSkillsFromCVAsync(userId, cvId);
        }

        public async Task SynchronizeEmployeesWithFlowcaseIds()
        {
            _logger.LogInformation($"Synchronizing employees with Flowcase id and CV id");
            await _flowcaseClient.GetUsersAsync();
        }
    }
}
