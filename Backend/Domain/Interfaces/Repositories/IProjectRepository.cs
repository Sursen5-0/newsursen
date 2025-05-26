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
        Task<IEnumerable<ProjectDTO>> GetProjects(bool includeActive = false);
        Task InsertProjects(IEnumerable<ProjectDTO> projects);
        Task UpdateProjects(IEnumerable<ProjectDTO> projects);
    }
}
