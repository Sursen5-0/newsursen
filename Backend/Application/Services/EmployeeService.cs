using Domain.Interfaces.ExternalClients;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Infrastructure.Severa;
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
            //get employees 
            var employees = await employeeRepository.GetEmployees();
            var contracts = new List<EmployeeContractDTO>();
            logger.LogInformation($"Synchronizing on {employees.Count()} employees contracts");
            foreach (var employee in employees)
            {
                var result = await severaClient.GetWorkContractByUserId(employee.Id);
                contracts.Add(result);
            }
            logger.LogInformation($"Done pulling data from Severa, starting insert to db");

            await employeeRepository.InsertEmployeeContracts(contracts);
            logger.LogInformation($"Done inserting data into db");

        }
    }
}
