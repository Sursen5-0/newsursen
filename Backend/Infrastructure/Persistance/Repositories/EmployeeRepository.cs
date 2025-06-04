using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Entra.Mappers;
using Infrastructure.Persistance.Mappers;
using Infrastructure.Persistance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<IEnumerable<EmployeeDTO>> GetEmployees(bool includeDisabled = true)
        {
            var employees = _db.Employees.Include(x => x.EmployeeContracts).AsQueryable();
            var date = DateOnly.FromDateTime(DateTime.Now);
            if (!includeDisabled)
            {
                employees = employees.Where(x => !x.EmployeeContracts.Any() || x.EmployeeContracts.Max(c => c.ToDate ?? DateOnly.MaxValue) >= date);
            }
            return await employees.Select(x => x.ToDto()).ToListAsync();
        }


        public async Task<IEnumerable<EmployeeDTO>> GetEmployeeWithoutSeveraIds(bool includeDisabled = true)
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
                var id = employeeDTOs.FirstOrDefault(x => x.Email == employee.Email)?.Id;
                if (id == null)
                {
                    _logger.LogWarning("Unable to find email for user with email: {Email}", employee.Email);
                    continue;
                }
                employee.SeveraId = id;
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

        public async Task<List<EmployeeDTO>> GetByEntraIdsAsync(IEnumerable<Guid> entraIds)
        {
            var entities = await _db.Employees
                .Where(e => entraIds.Contains(e.EntraId))
                .ToListAsync();
            return entities.Select(e => e.ToDto()).ToList();
        }

        public async Task InsertEmployeesAsync(IEnumerable<EmployeeDTO> dtos)
        {
            if (dtos == null) throw new ArgumentNullException(nameof(dtos));
            var inserts = dtos.Select(x => x.ToEntity());
            _db.Employees.AddRange(inserts);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateEmployeesAsync(IEnumerable<EmployeeDTO> dtos)
        {
            if (dtos == null) throw new ArgumentNullException(nameof(dtos));

            var entraIds = dtos.Select(d => d.EntraId).ToHashSet();
            var existingEntities = await _db.Employees
                .Where(e => entraIds.Contains(e.EntraId))
                .ToListAsync();

            var dbMap = existingEntities.ToDictionary(e => e.EntraId, e => e);
            var dtoMap = dtos.ToDictionary(d => d.EntraId, d => d);
            var now = DateTime.UtcNow;

            foreach (var entity in existingEntities)
            {
                var dto = dtoMap[entity.EntraId];
                var updated = dto.ToEntity();

                updated.Id = entity.Id;
                updated.CreatedAt = entity.CreatedAt;
                updated.UpdatedAt = now;

                _db.Entry(entity).CurrentValues.SetValues(updated);
            }

            await _db.SaveChangesAsync();
        }

        public async Task UpdateEmployeeSkills(IEnumerable<EmployeeSkillDTO> employeeSkills)
        {
            var skillList = _db.EmployeeSkills.ToDictionary(x => new { x.EmployeeId, x.ExternalId });
            foreach (var skill in employeeSkills)
            {
                var dbSkill = skillList[new { skill.EmployeeId, skill.ExternalId }];
                var updatedSkill = skill.ToEntity();
                updatedSkill.Id = dbSkill.Id;
                updatedSkill.UpdatedAt = DateTime.UtcNow;
                updatedSkill.CreatedAt = dbSkill.CreatedAt;
                _db.Entry(dbSkill).CurrentValues.SetValues(updatedSkill);
            }
            await _db.SaveChangesAsync();
        }

        public async Task InsertEmployeeSkills(IEnumerable<EmployeeSkillDTO> employeeSkills)
        {

            var employeeIdList = _db.Employees.Select(e => e.Id).ToList();
            var items = new List<EmployeeSkill>();
            foreach (var skill in employeeSkills)
            {
                if (!employeeIdList.Contains(skill.EmployeeId))
                {
                    _logger.LogWarning($"EmployeeId {skill.EmployeeId} not found for skills, not inserting data.");
                    continue;
                }
                items.Add(skill.ToEntity());
            }
            _db.EmployeeSkills.AddRange(items);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<EmployeeSkillDTO>> GetEmployeeSkills()
        {
            return await _db.EmployeeSkills.Select(x => x.ToDto()).ToListAsync();
        }

        public async Task DeleteEmployeeSkills(IEnumerable<Guid> ids)
        {
            var itemsToRemove = _db.EmployeeSkills.Where(x => ids.Contains(x.Id));
            _db.EmployeeSkills.RemoveRange(itemsToRemove);
            await _db.SaveChangesAsync();
        }
    }
}
