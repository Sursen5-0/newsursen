using Application.Services;
using Domain.Interfaces.Services;
using Domain.Models;
using Infrastructure.Persistance.Models;
using Infrastructure.Severa;
using Infrastructure.Severa.Models;

namespace Hangfire.Jobs
{
    public class EntraJobs(IEmployeeService employeeService)
    {

        public async Task GetAllEmployeesEntra()
        {
            await employeeService.SynchronizeEmployeesAsync();
        }
    }
}
