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
        public async Task SynchronizeAbsence()
        {
            _logger.LogInformation($"Start synchronizing of absence");
            var absences = await _severaClient.GetAbsence();
            if (absences == null || absences.Any())
            {
                _logger.LogWarning($"_severaClient.GetAbsence returned empty, not continuing syncronization of absence");
                return;
            }
            var employees = await _employeeRepository.GetEmployees();
            var dbAbsences = await _employeeRepository.GetAbsenceByExternalIDs(absences.Select(x => x.ExternalId));
            var dbExternalIds = new HashSet<Guid>(dbAbsences.Select(x => x.ExternalId));

            var absenceArray = absences.ToArray();
            for (int i = 0; i < absences.Count(); i++)
            {
                var employee = employees.FirstOrDefault(x => x.SeveraId == absenceArray[i].SeveraEmployeeId);
                if (employee == null)
                {
                    _logger.LogError("No employeeId found for the severa user with ID: {absence.ExternalId}", absenceArray[i].ExternalId);
                    continue;
                }
                absenceArray[i].EmployeeId = employee.Id;
            }


            var updateList = new List<AbsenceDTO>();
            var insertList = new List<AbsenceDTO>();
            foreach (var item in absences)
            {
                if (dbExternalIds.Any(x => x == item.ExternalId))
                {
                    updateList.Add(item);
                }
                else
                {
                    insertList.Add(item);
                }
            }



            insertList = insertList.Where(x => x.EmployeeId.HasValue).ToList();
            _logger.LogInformation("Updating {updateamount}", updateList.Count);
            await _employeeRepository.UpdateAbsences(updateList);
            _logger.LogInformation("Inserting {insertamount}", insertList.Count);
            await _employeeRepository.InsertAbsences(insertList);
        }

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
