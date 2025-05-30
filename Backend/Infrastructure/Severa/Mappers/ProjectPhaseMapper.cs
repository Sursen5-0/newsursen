using Domain.Models;
using Infrastructure.Severa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Severa.Mappers
{
    public static class ProjectPhaseMapper
    {
        public static ProjectPhaseDTO ToDto(this SeveraPhaseModel model)
        {
            return new ProjectPhaseDTO()
            {
                Name = model.Name,
                DeadLine = model.DeadLine,
                ExternalId = model.ExternalId,
                ExternalParentPhaseId = model.ParentPhaseId,
                ExternalProjectId = model.ExternalProjectId,
                StartDate = model.StartDate,
            };
        }
    }
}
