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
        public async Task<IEnumerable<ProjectDTO>> GetProjects(bool includeActive = false)
        {
            var projects = _db.Projects.AsQueryable();
            if (!includeActive)
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
            _db.AddRange(items);
            await _db.SaveChangesAsync();
        }
    }
}
