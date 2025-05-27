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
    public class EmployeeRepository(SursenContext _db, ILogger<EmployeeRepository> _logger) : IEmployeeRepository
    {
        public async Task InsertEmployeeContracts(IEnumerable<EmployeeContractDTO> employeeContractDtos)
        {
            ArgumentNullException.ThrowIfNull(employeeContractDtos);

            var employeeIdList = _db.Employees.Select(e => e.Id).ToList();
            var contractList = _db.EmployeeContracts.Where(x => employeeContractDtos.Select(c => c.Id).Contains(x.Id)).ToDictionary(x => x.Id);

            foreach (var contract in employeeContractDtos)
            {
                if (!employeeIdList.Contains(contract.EmployeeId))
                {
                    _logger.LogWarning($"EmployeeId {contract.EmployeeId} not found for contracts, not inserting data. SeveraID is {contract.SeveraId}");
                    continue;
                }
                if (contract.Id == default || !contractList.ContainsKey(contract.Id))
                {
                    await _db.EmployeeContracts.AddAsync(contract.ToEntity());
                }
                else
                {
                    var dbcontract = contractList[contract.Id];
                    _db.Entry(dbcontract).CurrentValues.SetValues(contract.ToEntity());
                }
            }
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<EmployeeDTO>> GetEmployees(bool includeDisabled = false)
        {
            var employees = _db.Employees.Include(x => x.EmployeeContracts).AsQueryable();
            var date = DateOnly.FromDateTime(DateTime.Now);
            if (!includeDisabled)
            {
                employees = employees.Where(x => !x.EmployeeContracts.Any() || x.EmployeeContracts.Max(c => c.ToDate ?? DateOnly.MaxValue) >= date);
            }
            return await employees.Select(x => x.ToDto()).ToListAsync();
        }


        public async Task<IEnumerable<EmployeeDTO>> GetEmployeeWithoutSeveraIds(bool includeDisabled = false)
        {
            var employees = _db.Employees.Include(x => x.EmployeeContracts).Where(x => x.SeveraId == null).AsQueryable();
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
            var employees = _db.Employees.Where(x => employeeEmails.Contains(x.Email));
            foreach (var employee in employees)
            {
                employee.SeveraId = employeeDTOs.First(x => x.Email == employee.Email).Id;
            }
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<AbsenceDTO>> GetAbsenceByExternalIDs(IEnumerable<Guid> ids)
        {
            return await _db.Absences.Where(x => ids.Contains(x.ExternalId)).Select(x => x.ToDto()).ToListAsync();

        }

        public async Task UpdateAbsences(IEnumerable<AbsenceDTO> absences)
        {
            _logger.LogInformation("Updating {Count} absences", absences.Count());
            var absencesIds = absences.Select(x => x.ExternalId).ToHashSet();
            var dbAbsence = _db.Absences.Where(x => absencesIds.Contains(x.ExternalId)).ToDictionary(x => x.ExternalId);
            var absencesFixed = absences.Select(x => x.ToEntity()).ToDictionary(x => x.ExternalId);
            var nowDate = DateTime.Now;
            foreach (var absence in dbAbsence)
            {
                if (absencesFixed.TryGetValue(absence.Key, out var updatedAbsence))
                {
                    updatedAbsence.Id = absence.Value.Id;
                    updatedAbsence.CreatedAt = absence.Value.CreatedAt;
                    updatedAbsence.UpdatedAt = nowDate;
                    _db.Entry(absence.Value).CurrentValues.SetValues(updatedAbsence);
                }
                else
                {
                    _logger.LogError("Absence intended for update not found in input list. ExternalId: {absence.Key}", absence.Key);
                }
            }
            await _db.SaveChangesAsync();
        }

        public async Task InsertAbsences(IEnumerable<AbsenceDTO> absences)
        {
            _logger.LogInformation("Inserting {Count} new absences", absences.Count());
            _db.Absences.AddRange(absences.Select(x => x.ToEntity()));
            await _db.SaveChangesAsync();
        }


        public async Task InsertOrUpdateEmployees(IEnumerable<EmployeeDTO> dtos)
        {
            if (dtos == null) throw new ArgumentNullException(nameof(dtos));

            var existingIds = await _db.Employees
                .AsNoTracking()
                .Select(e => e.EntraId)
                .ToListAsync();

            foreach (var dto in dtos)
            {
                if (dto == null)
                {
                    _logger.LogWarning("Skipped null EmployeeDTO");
                    continue;
                }

                var entity = dto.ToEntity();
                if (existingIds.Contains(dto.EntraId))
                {
                    
                    var tracked = await _db.Employees
                        .FirstAsync(e => e.EntraId == dto.EntraId);

                    _db.Entry(tracked).CurrentValues.SetValues(entity);
                    _logger.LogInformation("Updated employee with EntraId {EntraId}", dto.EntraId);
                }
                else
                {
                    await _db.Employees.AddAsync(entity);
                    _logger.LogInformation("Inserted new employee with EntraId {EntraId}", dto.EntraId);
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}
