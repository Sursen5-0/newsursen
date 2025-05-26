using Application.Services;
using Domain.Interfaces.Services;
using Domain.Models;
using Infrastructure.Persistance.Models;
using Infrastructure.Severa;
using Infrastructure.Severa.Models;

namespace Hangfire.Jobs
{
    public class SeveraJobs(IEmployeeService employeeService)
    {
        public async Task SynchronizeContracts()
        {
            await employeeService.SynchronizeContracts();
        }
        public async Task SynchronizeEmployees()
        {
            await employeeService.SynchronizeUnmappedSeveraIds();
        }

        public async Task SynchronizeAbsence()
        {
            await employeeService.SynchronizeAbsence();
        }
    }
}
