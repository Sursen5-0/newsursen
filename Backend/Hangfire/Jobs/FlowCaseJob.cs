using Domain.Interfaces.ExternalClients;
using Infrastructure.FlowCase;
using Domain.Interfaces.Services;


namespace HangFire.Jobs
{
    public class FlowCaseJob(ISkillService skillService)
    {
        public async Task SynchronizeSkills()
        {
            await skillService.SynchronizeSkillsFromFlowcaseAsync();
        }
    }
}