using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Repositories
{
    public interface IEmployeeRepository
    {
        /// <summary>
        /// Retrieves all employees from the database, optionally including disabled employees.
        /// </summary>
        /// <param name="includeDisabled">If <c>true</c>, includes disabled employees; otherwise, only active employees are returned.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a collection of <see cref="EmployeeDTO"/> objects.
        /// </returns>
        Task<IEnumerable<EmployeeDTO>> GetEmployees(bool includeDisabled = true);
        /// <summary>
        /// Retrieves employees who do not have an associated Severa ID, optionally including disabled employees.
        /// </summary>
        /// <param name="includeDisabled">If <c>true</c>, includes disabled employees; otherwise, only active employees are returned.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a collection of <see cref="EmployeeDTO"/> objects without Severa IDs.
        /// </returns>
        Task<IEnumerable<EmployeeDTO>> GetEmployeeWithoutSeveraIds(bool includeDisabled = true);
        /// <summary>
        /// Retrieves absences from the database by their external IDs.
        /// </summary>
        /// <param name="ids">A collection of external absence IDs to search for.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a collection of <see cref="AbsenceDTO"/> objects matching the provided IDs.
        /// </returns>
        Task<IEnumerable<AbsenceDTO>> GetAbsenceByExternalIDs(IEnumerable<Guid> ids);
        /// <summary>
        /// Inserts new or updates existing employee contract records in the database.
        /// Contracts are matched by their ID; if not found, a new contract is inserted.
        /// </summary>
        /// <param name="employeeContractDtos">A collection of <see cref="EmployeeContractDTO"/> objects to insert or update.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task InsertEmployeeContracts(IEnumerable<EmployeeContractDTO> employeeDTO);
        /// <summary>
        /// Updates the Severa IDs for employees in the database based on their email addresses.
        /// </summary>
        /// <param name="employeeDTOs">A collection of <see cref="SeveraEmployeeModel"/> objects containing updated Severa IDs and emails.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateSeveraIds(IEnumerable<SeveraEmployeeModel> employeeDTO);
        /// <summary>
        /// Updates existing absence records in the database.
        /// </summary>
        /// <param name="absences">A collection of <see cref="AbsenceDTO"/> objects to update.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateAbsences(IEnumerable<AbsenceDTO> absences);
        /// <summary>
        /// Inserts new absence records into the database.
        /// </summary>
        /// <param name="absences">A collection of <see cref="AbsenceDTO"/> objects to insert.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task InsertAbsences(IEnumerable<AbsenceDTO> absences);
        /// <summary>
        /// Retrieves employees from the database by their Azure Entra IDs.
        /// </summary>
        /// <param name="entraIds">A collection of Entra GUIDs to search for.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a list of <see cref="EmployeeDTO"/> objects matching the provided Entra IDs.
        /// </returns>
        Task<List<EmployeeDTO>> GetByEntraIdsAsync(IEnumerable<Guid> entraIds);
        /// <summary>
        /// Inserts new employee records into the database.
        /// </summary>
        /// <param name="dtos">A collection of <see cref="EmployeeDTO"/> objects to insert.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task InsertEmployeesAsync(IEnumerable<EmployeeDTO> dtos);
        /// <summary>
        /// Updates existing employee records in the database.
        /// </summary>
        /// <param name="dtos">A collection of <see cref="EmployeeDTO"/> objects to update.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateEmployeesAsync(IEnumerable<EmployeeDTO> dtos);
        /// <summary>
        /// Inserts new employee skill records into the database.
        /// </summary>
        /// <param name="employeeSkills">A collection of <see cref="EmployeeSkillDTO"/> objects to insert.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task InsertEmployeeSkills(IEnumerable<EmployeeSkillDTO> employeeSkills);
        /// <summary>
        /// Updates existing employee skill records in the database.
        /// </summary>
        /// <param name="employeeSkills">A collection of <see cref="EmployeeSkillDTO"/> objects to update.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateEmployeeSkills(IEnumerable<EmployeeSkillDTO> employeeSkills);
        /// <summary>
        /// Retrieves all employee skills from the database.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a collection of <see cref="EmployeeSkillDTO"/> objects.
        /// </returns>
        Task<IEnumerable<EmployeeSkillDTO>> GetEmployeeSkills();
        /// <summary>
        /// Deletes employee skills from the database by their skill IDs.
        /// </summary>
        /// <param name="skillIds">A collection of skill record GUIDs to delete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DeleteEmployeeSkills(IEnumerable<Guid> skillIds);
    }
}
