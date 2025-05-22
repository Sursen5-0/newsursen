using Domain.Interfaces.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistance.Mappers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Contracts;

namespace Infrastructure.Persistance.Repositories
{
    public class EmployeeRepository(SursenContext db, ILogger<EmployeeRepository> logger) : IEmployeeRepository
    {
        public async Task InsertEmployeeContracts(IEnumerable<EmployeeContractDTO> employeeContractDtos)
        {
            ArgumentNullException.ThrowIfNull(employeeContractDtos);

            var employeeIdList = db.Employees.Select(e => e.Id).ToList();
            var contractList = db.EmployeeContracts.Where(x => employeeContractDtos.Select(c => c.Id).Contains(x.Id)).ToDictionary(x => x.Id);

            foreach (var contract in employeeContractDtos)
            {
                if (!employeeIdList.Contains(contract.EmployeeId))
                {
                    logger.LogWarning($"EmployeeId {contract.EmployeeId} not found for contracts, not inserting data. SeveraID is {contract.SeveraId}");
                    continue;
                }
                if (contract.Id == default || !contractList.ContainsKey(contract.Id))
                {
                    await db.EmployeeContracts.AddAsync(contract.ToEntity());
                }
                else
                {
                    var dbcontract = contractList[contract.Id];
                    db.Entry(dbcontract).CurrentValues.SetValues(contract.ToEntity());
                }
            }
            await db.SaveChangesAsync();
        }

        public async Task<IEnumerable<EmployeeDTO>> GetEmployees(bool includeDisabled = false)
        {
            var employees = db.Employees.Include(x => x.EmployeeContracts).AsQueryable();
            var date = DateOnly.FromDateTime(DateTime.Now);
            if (!includeDisabled)
            {
                employees = employees.Where(x => !x.EmployeeContracts.Any() || x.EmployeeContracts.Max(c => c.ToDate ?? DateOnly.MaxValue) >= date);
            }
            return await employees.Select(x => x.ToDto()).ToListAsync();
        }


        public async Task<IEnumerable<EmployeeDTO>> GetEmployeeWithoutSeveraIds(bool includeDisabled = false)
        {
            var employees = db.Employees.Include(x => x.EmployeeContracts).Where(x => x.SeveraId == null).AsQueryable();
            var date = DateOnly.FromDateTime(DateTime.Now);
            if (!includeDisabled)
            {
                employees = employees.Where(x => !x.EmployeeContracts.Any() || x.EmployeeContracts.Max(c => c.ToDate ?? DateOnly.MaxValue) >= date);
            }
            return await employees.Select(x => x.ToDto()).ToListAsync();
        }

        public async Task UpdateSeveraIds(IEnumerable<SeveraEmployeeModel> employeeDTOs)
        {
            var employeeEmails = employeeDTOs.Select(x => x.Email);
            var employees = db.Employees.Where(x => employeeEmails.Contains(x.Email));
            foreach (var employee in employees) 
            {
                employee.SeveraId = employeeDTOs.First(x => x.Email == employee.Email).Id;
            }
            await db.SaveChangesAsync();
        }
    }
}
