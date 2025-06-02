using Domain.Interfaces.ExternalClients;
using Infrastructure.FlowCase;
using Domain.Interfaces.Services;


namespace HangFire.Jobs
{
    public class FlowCaseJob(ISkillService skillService, IEmployeeService employeeService)
    {
        public async Task SynchronizeSkillsToSkillsTable()
        {
            await skillService.SynchronizeSkillsFromFlowcaseAsync();
        }

        public async Task SynchronizeEmployeesWithFlowcaseIdsAsync()
        {
            await employeeService.SynchronizeEmployeesWithFlowcaseIdsAsync();
        }

        public async Task SynchronizeEmployeeSkillsAsync() 
        { 
            await employeeService.SynchronizeEmployeeSkillsAsync();
        }
    }
}