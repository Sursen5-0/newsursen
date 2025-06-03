using Application.Services;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Infrastructure.Persistance.Models;
using Infrastructure.Severa;
using Infrastructure.Severa.Models;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Hangfire.Jobs
{
    public class SeveraJobs(IEmployeeService _employeeService, IJobExecutionRepository _jobExecutionRepository)
    {
        public async Task SynchronizeContracts()
        {
            var result = await JobLogger.LogJobExecutionAsync(nameof(SynchronizeContracts), _employeeService.SynchronizeContracts());
            await _jobExecutionRepository.InsertJobExecution(result);
            JobLogger.ThrowIfFailed(result);
        }

        public async Task SynchronizeEmployees()
        {
            var result = await JobLogger.LogJobExecutionAsync(nameof(SynchronizeEmployees), _employeeService.SynchronizeUnmappedSeveraIds());
            await _jobExecutionRepository.InsertJobExecution(result);
            JobLogger.ThrowIfFailed(result);
        }

        public async Task SynchronizeAbsence()
        {
            var result = await JobLogger.LogJobExecutionAsync(nameof(SynchronizeAbsence), _employeeService.SynchronizeAbsence());
            await _jobExecutionRepository.InsertJobExecution(result);
            JobLogger.ThrowIfFailed(result);
        }

        public async Task SynchronizeProjects()
        {
            var result = await JobLogger.LogJobExecutionAsync(nameof(SynchronizeProjects), _employeeService.SynchronizeProjects());
            await _jobExecutionRepository.InsertJobExecution(result);
            JobLogger.ThrowIfFailed(result);
        }

        public async Task SynchronizePhases()
        {
            var result = await JobLogger.LogJobExecutionAsync(nameof(SynchronizePhases), _employeeService.SynchronizePhases());
            await _jobExecutionRepository.InsertJobExecution(result);
            JobLogger.ThrowIfFailed(result);
        }

    }
}
