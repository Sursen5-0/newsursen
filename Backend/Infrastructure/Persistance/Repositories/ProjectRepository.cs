using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistance.Mappers;
using Infrastructure.Persistance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Repositories
{


    public class ProjectRepository(SursenContext _db, ILogger<ProjectRepository> _logger) : IProjectRepository
    {
        public async Task<IEnumerable<ProjectDTO>> GetProjects(bool includeInactive = true)
        {
            var projects = _db.Projects.AsQueryable();
            if (!includeInactive)
            {
                projects = projects.Where(x => !x.IsClosed);
            }
            return await projects.Select(x => x.ToDto()).ToListAsync();
        }

        public async Task UpdateProjects(IEnumerable<ProjectDTO> projects)
        {
            _logger.LogInformation("Updating {Count} projects", projects.Count());
            var projectsIds = projects.Select(x => x.ExternalId).ToHashSet();
            var dbpRojects = _db.Projects.Where(x => projectsIds.Contains(x.ExternalId)).ToDictionary(x => x.ExternalId);
            var projectsFixed = projects.Select(x => x.ToEntity()).ToDictionary(x => x.ExternalId);
            var nowDate = DateTime.Now;
            foreach (var project in dbpRojects)
            {
                if (projectsFixed.TryGetValue(project.Key, out var updatedProject))
                {
                    updatedProject.Id = project.Value.Id;
                    updatedProject.CreatedAt = project.Value.CreatedAt;
                    updatedProject.UpdatedAt = nowDate;
                    _db.Entry(project.Value).CurrentValues.SetValues(updatedProject);
                }
                else
                {
                    _logger.LogError("Project intended for update not found in input list. ExternalId: {project.Key}", project.Key);
                }
            }
            await _db.SaveChangesAsync();

        }
        public async Task InsertProjects(IEnumerable<ProjectDTO> projects)
        {
            _logger.LogInformation("Inserting {Count} new projects", projects.Count());

            var items = projects.Select(x => x.ToEntity());
            _db.Projects.AddRange(items);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProjectPhaseDTO>> GetPhases(bool includeInactive = true)
        {
            var phases = _db.ProjectPhases.AsQueryable();
            if (!includeInactive)
            {
                phases = phases.Where(x => x.DeadLine < DateTime.Now);
            }
            return await phases.Select(x => x.ToDto()).ToListAsync();
        }

        public async Task InsertPhases(IEnumerable<ProjectPhaseDTO> phases)
        {
            _logger.LogInformation("Inserting {Count} new phases", phases.Count());
            var items = phases.ToDictionary(x =>x, x=> x.ToEntity());

            var entityLookup = items.ToDictionary(x => x.Value.ExternalId);
            foreach (var item in items)
            {
                if (item.Key.ExternalParentPhaseId == null)
                {
                    continue;
                }
                item.Value.ParentPhaseId = entityLookup[item.Key.ExternalParentPhaseId.Value].Value.Id;
            }

            _db.ProjectPhases.AddRange(items.Values);
            await _db.SaveChangesAsync();

        }

        public async Task UpdatePhases(IEnumerable<ProjectPhaseDTO> phases)
        {
            _logger.LogInformation("Updating {Count} phases", phases.Count());
            var phaseIds = phases.Select(x => x.ExternalId).ToHashSet();
            var dbPhases = _db.ProjectPhases.Where(x => phaseIds.Contains(x.ExternalId)).ToDictionary(x => x.ExternalId);
            var phaseFixed = phases.Select(x => x.ToEntity()).ToDictionary(x => x.ExternalId);
            var nowDate = DateTime.Now;
            foreach (var phase in dbPhases)
            {
                if (phaseFixed.TryGetValue(phase.Key, out var updatedProject))
                {
                    updatedProject.Id = phase.Value.Id;
                    updatedProject.CreatedAt = phase.Value.CreatedAt;
                    updatedProject.UpdatedAt = nowDate;
                    _db.ProjectPhases.Entry(phase.Value).CurrentValues.SetValues(updatedProject);
                }
                else
                {
                    _logger.LogError("Phases intended for update not found in input list. ExternalId: {project.Key}", phase.Key);
                }
            }
            await _db.SaveChangesAsync();

        }
    }
}
