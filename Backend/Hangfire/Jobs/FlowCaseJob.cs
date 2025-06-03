using Domain.Interfaces.ExternalClients;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Hangfire;
using Infrastructure.FlowCase;


namespace Hangfire.Jobs
{
    public class FlowCaseJob(ISkillService skillService, IEmployeeService employeeService, IJobExecutionRepository _jobExecutionRepository)
    {
        public async Task SynchronizeSkillsToSkillsTable()
        {
            var result = await JobLogger.LogJobExecutionAsync(nameof(SynchronizeSkillsToSkillsTable), skillService.SynchronizeSkillsFromFlowcaseAsync());
            await _jobExecutionRepository.InsertJobExecution(result);
            JobLogger.ThrowIfFailed(result);
        }

        public async Task SynchronizeEmployeesWithFlowcaseIdsAsync()
        {
            var result = await JobLogger.LogJobExecutionAsync(nameof(SynchronizeEmployeesWithFlowcaseIdsAsync), employeeService.SynchronizeEmployeesWithFlowcaseIdsAsync());
            await _jobExecutionRepository.InsertJobExecution(result);
            JobLogger.ThrowIfFailed(result);
        }

        public async Task SynchronizeEmployeeSkillsAsync() 
        { 
            var result = await JobLogger.LogJobExecutionAsync(nameof(SynchronizeEmployeeSkillsAsync), employeeService.SynchronizeEmployeeSkillsAsync());
            await _jobExecutionRepository.InsertJobExecution(result);
            JobLogger.ThrowIfFailed(result);
        }
    }
}