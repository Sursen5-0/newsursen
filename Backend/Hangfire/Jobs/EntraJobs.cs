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
    public class EntraJobs(IEmployeeService _employeeService,IJobExecutionRepository _jobExecutionRepository)
    {

        public async Task GetAllEmployeesEntra()
        {
            var result = await JobLogger.LogJobExecutionAsync(MethodBase.GetCurrentMethod()?.Name ?? "Unknown method", _employeeService.SynchronizeEmployeesAsync());
            await _jobExecutionRepository.InsertJobExecution(result);

        }
    }
}
