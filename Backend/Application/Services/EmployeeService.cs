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
    public class EmployeeService(ISeveraClient _severaClient, IEmployeeRepository _employeeRepository, ILogger<EmployeeService> _logger) : IEmployeeService
    {
        public async Task SynchronizeContracts()
        {
            _logger.LogInformation($"Start synchronizing of contracts");

            var employees = await _employeeRepository.GetEmployees();
            employees = employees.Where(x => x.SeveraId != null);
            var contracts = new List<EmployeeContractDTO>();
            _logger.LogInformation($"Synchronizing on {employees.Count()} employees contracts");

            foreach (var employee in employees)
            {
                var result = await _severaClient.GetWorkContractByUserId(employee.SeveraId!.Value);
                result.EmployeeId = employee.Id;
                contracts.Add(result);
            }

            _logger.LogInformation($"Done pulling data from Severa, starting insert to db");
            await _employeeRepository.InsertEmployeeContracts(contracts);

            _logger.LogInformation($"Done synchronizing contracts");
        }

        public async Task SynchronizeUnmappedSeveraIds()
        {
            _logger.LogInformation($"Start synchronizing of severaIds");
            var dbEmployees = await _employeeRepository.GetEmployeeWithoutSeveraIds();
            var severaEmployees = new List<SeveraEmployeeModel>();
            foreach (var employee in dbEmployees)
            {
                var data = await _severaClient.GetUserByEmail(employee.Email);
                if (data == null)
                {
                    continue;
                }
                severaEmployees.Add(data);
            }
            await _employeeRepository.UpdateSeveraIds(severaEmployees);
        }
    }
}
