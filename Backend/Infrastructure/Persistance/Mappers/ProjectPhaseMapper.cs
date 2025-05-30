using Domain.Models;
using Infrastructure.Persistance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Mappers
{
    public static class ProjectPhaseMapper
    {
        public static ProjectPhaseDTO ToDto(this ProjectPhase projectPhase)
        {
            return new ProjectPhaseDTO()
            {
                Id = projectPhase.Id,
                Name = projectPhase.Name,
                DeadLine = projectPhase.DeadLine,
                ExternalId = projectPhase.ExternalId,
                ProjectId = projectPhase.ProjectId,
                ParentPhaseId = projectPhase.ParentPhaseId,
                StartDate = projectPhase.StartDate,
            };
        }
        public static ProjectPhase ToEntity(this ProjectPhaseDTO projectPhase)
        {
            return new ProjectPhase()
            {
                Id =  Guid.NewGuid(),
                Name = projectPhase.Name,
                DeadLine = projectPhase.DeadLine,
                ExternalId = projectPhase.ExternalId,
                ProjectId = projectPhase.ProjectId,
                ParentPhaseId = projectPhase.ParentPhaseId,
                StartDate = projectPhase.StartDate,
            };
        }

    }
}
