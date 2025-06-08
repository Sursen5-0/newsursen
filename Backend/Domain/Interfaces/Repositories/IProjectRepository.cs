using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Repositories
{
    public interface IProjectRepository
    {
        /// <summary>
        /// Retrieves all projects from the database, optionally including inactive (closed) projects.
        /// </summary>
        /// <param name="includeInactive">
        /// If <c>true</c>, includes all projects; otherwise, only projects that are not closed are returned.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a collection of <see cref="ProjectDTO"/> objects.
        /// </returns>
        Task<IEnumerable<ProjectDTO>> GetProjects(bool includeInactive = true);
        /// <summary>
        /// Inserts new project records into the database.
        /// </summary>
        /// <param name="projects">A collection of <see cref="ProjectDTO"/> objects to insert.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task InsertProjects(IEnumerable<ProjectDTO> projects);
        /// <summary>
        /// Updates existing project records in the database.
        /// </summary>
        /// <param name="projects">A collection of <see cref="ProjectDTO"/> objects to update.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task UpdateProjects(IEnumerable<ProjectDTO> projects);
        /// <summary>
        /// Retrieves all project phases from the database, optionally including inactive phases.
        /// </summary>
        /// <param name="includeInactive">
        /// If <c>true</c>, includes all phases; otherwise, only phases with a deadline in the future are returned.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a collection of <see cref="ProjectPhaseDTO"/> objects.
        /// </returns>
        Task<IEnumerable<ProjectPhaseDTO>> GetPhases(bool includeInactive = true);
        /// <summary>
        /// Inserts new project phase records into the database.
        /// </summary>
        /// <param name="phases">A collection of <see cref="ProjectPhaseDTO"/> objects to insert.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task InsertPhases(IEnumerable<ProjectPhaseDTO> phases);
        /// <summary>
        /// Updates existing project phase records in the database.
        /// </summary>
        /// <param name="phases">A collection of <see cref="ProjectPhaseDTO"/> objects to update.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task UpdatePhases(IEnumerable<ProjectPhaseDTO> phases);
    }
}
