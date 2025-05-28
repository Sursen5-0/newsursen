using Domain.Interfaces.ExternalClients;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class SkillService(IFlowCaseClient _flowcaseClient, ISkillRepository _skillRepository, ILogger<SkillService> _logger) : ISkillService
    {
        public async Task SynchronizeSkillsFromFlowcaseAsync()
        {
            ArgumentNullException.ThrowIfNull(_flowcaseClient);
            ArgumentNullException.ThrowIfNull(_skillRepository);
            ArgumentNullException.ThrowIfNull(_logger);

            _logger.LogInformation("Synchronizing skills from Flowcase");
            
            List<SkillDTO> skills = await _flowcaseClient.GetSkillsFromFlowcaseAsync();
            if (skills == null || !skills.Any())
            {
                _logger.LogWarning("No skills found in Flowcase, skipping synchronization.");
                return;
            }
            
            var existingSkills = await _skillRepository.GetAllSkillsAsync();
            var existingSkillNames = existingSkills.Select(s => s.SkillName).ToHashSet();

            foreach (var skill in skills) 
            {
                if (!existingSkillNames.Contains(skill.SkillName))
                {
                    _logger.LogInformation($"Adding new skill: {skill.SkillName}");
                    await _skillRepository.AddSkillAsync(skill);
                }
                else
                {
                    _logger.LogInformation($"Skill already exists: {skill.SkillName}, skipping addition.");
                }
            }
        }
    }
}
