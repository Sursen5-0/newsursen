using Application.Services;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Infrastructure.Persistance.Models;
using Infrastructure.Severa;
using Infrastructure.Severa.Models;
using System.Reflection;

namespace Hangfire.Jobs
{
    public class SeveraJobs(IEmployeeService _employeeService, IJobExecutionRepository _jobExecutionRepository)
    {
        public async Task SynchronizeContracts()
        {
            var result = await JobLogger.LogJobExecutionAsync(MethodBase.GetCurrentMethod()?.Name ?? "Unknown method", _employeeService.SynchronizeContracts());
            _jobExecutionRepository.InsertJobExecution(result);

        }
        public async Task SynchronizeEmployees()
        {
            var result = await JobLogger.LogJobExecutionAsync(MethodBase.GetCurrentMethod()?.Name ?? "Unknown method", _employeeService.SynchronizeUnmappedSeveraIds());
            _jobExecutionRepository.InsertJobExecution(result);

        }

        public async Task SynchronizeAbsence()
        {
            var result = await JobLogger.LogJobExecutionAsync(MethodBase.GetCurrentMethod()?.Name ?? "Unknown method", _employeeService.SynchronizeAbsence());
            _jobExecutionRepository.InsertJobExecution(result);
        }

        public async Task SynchronizeProjects()
        {
            var result = await JobLogger.LogJobExecutionAsync(MethodBase.GetCurrentMethod()?.Name ?? "Unknown method", _employeeService.SynchronizeProjects());
            _jobExecutionRepository.InsertJobExecution(result);

        }
        public async Task SynchronizePhases()
        {
            var result = await JobLogger.LogJobExecutionAsync(MethodBase.GetCurrentMethod()?.Name ?? "Unknown method", _employeeService.SynchronizePhases());
            _jobExecutionRepository.InsertJobExecution(result);

        }

    }
}
