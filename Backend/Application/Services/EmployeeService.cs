using Domain.Interfaces.ExternalClients;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class EmployeeService(ISeveraClient severaClient, IEmployeeRepository employeeRepository, ILogger<EmployeeService> logger) : IEmployeeService
    {
        public async Task SynchronizeContracts()
        {
            logger.LogInformation($"Start synchronizing of contracts");

            var employees = await employeeRepository.GetEmployees();
            employees = employees.Where(x => x.SeveraId != null);
            var contracts = new List<EmployeeContractDTO>();
            logger.LogInformation($"Synchronizing on {employees.Count()} employees contracts");

            foreach (var employee in employees)
            {
                var result = await severaClient.GetWorkContractByUserId(employee.SeveraId!.Value);
                result.EmployeeId = employee.Id;
                contracts.Add(result);
            }

            logger.LogInformation($"Done pulling data from Severa, starting insert to db");
            await employeeRepository.InsertEmployeeContracts(contracts);

            logger.LogInformation($"Done synchronizing contracts");
        }

        public async Task SynchronizeUnmappedSeveraIds()
        {
            logger.LogInformation($"Start synchronizing of severaIds");
            var dbEmployees = await employeeRepository.GetEmployeeWithoutSeveraIds();
            var severaEmployees = new List<SeveraEmployeeModel>();
            foreach (var employee in dbEmployees)
            {
                var data = await severaClient.GetUserByEmail(employee.Email);
                if (data == null)
                {
                    continue;
                }
                severaEmployees.Add(data);
            }
            await employeeRepository.UpdateSeveraIds(severaEmployees);
        }
    }
}
