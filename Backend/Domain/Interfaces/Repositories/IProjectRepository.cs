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
        Task<IEnumerable<ProjectDTO>> GetProjects(bool includeInactive = true);
        Task InsertProjects(IEnumerable<ProjectDTO> projects);
        Task UpdateProjects(IEnumerable<ProjectDTO> projects);
        Task<IEnumerable<ProjectPhaseDTO>> GetPhases(bool includeInactive = true);
        Task InsertPhases(IEnumerable<ProjectPhaseDTO> phases);
        Task UpdatePhases(IEnumerable<ProjectPhaseDTO> phases);
    }
}
