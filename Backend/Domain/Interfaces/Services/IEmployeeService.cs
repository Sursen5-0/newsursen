using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Services
{
    public interface IEmployeeService
    {
        /// <summary>
        /// Synchronizes employee contract records between Severa and the local database.
        /// Retrieves contracts for all employees with a Severa ID and inserts or updates them in the local database.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task SynchronizeContracts();
        /// <summary>
        /// Synchronizes unmapped Severa IDs for employees in the local database.
        /// Retrieves employees without a Severa ID, attempts to find their Severa user by email,
        /// and updates their records with the corresponding Severa ID.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task SynchronizeUnmappedSeveraIds();
        /// <summary>
        /// Synchronizes absence records between Severa and the local database.
        /// Retrieves absences from Severa since the last successful synchronization, matches them to employees,
        /// and updates or inserts records in the local database as needed.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task SynchronizeAbsence();
        /// <summary>
        /// Synchronizes employee records between Entra and the local database.
        /// Retrieves all employees from Entra, determines which records to insert or update,
        /// and updates manager relationships as needed.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task SynchronizeEmployeesAsync();
        /// <summary>
        /// Synchronizes project records between Severa and the local database.
        /// Retrieves projects from Severa since the last successful synchronization, matches them to employees,
        /// and inserts or updates records in the local database as needed.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task SynchronizeProjects();
        /// <summary>
        /// Synchronizes project phases between Severa and the local database.
        /// Retrieves phases from Severa since the last successful synchronization, matches them to projects,
        /// and inserts or updates records in the local database as needed.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task SynchronizePhases();
        /// <summary>
        /// Synchronizes employees' FlowCase IDs and CV IDs from FlowCase to the local database.
        /// Updates existing employee records with FlowCase and CV IDs if they are missing.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task SynchronizeEmployeesWithFlowcaseIdsAsync();
        /// <summary>
        /// Synchronizes employee skills between FlowCase and the local database.
        /// Retrieves skills for each employee with valid FlowCase and CV IDs, and inserts, updates, or deletes skills as needed.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task SynchronizeEmployeeSkillsAsync();
    }
}
