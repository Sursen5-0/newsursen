using Domain.Models;
using Infrastructure.Persistance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Mappers
{
    public static class ProjectMapper
    {
        public static ProjectDTO ToDto(this Project project)
        {
            return new ProjectDTO()
            {
                Id = project.Id,
                Description = project.Description,
                ExternalId = project.ExternalId,
                Name = project.Name,
                OwnerId = project.ResponsibleId,
                ExternalOwnerId = project.ExternalResponsibleId,
                IsClosed = project.IsClosed,
            };
        }
        public static Project ToEntity(this ProjectDTO project)
        {
            return new Project()
            {
                Id = Guid.NewGuid(),
                Description = project.Description,
                ExternalId = project.ExternalId,
                Name = project.Name,
                ResponsibleId = project.OwnerId,
                ExternalResponsibleId = project.ExternalOwnerId,
                IsClosed=project.IsClosed,                
            };
        }

    }
}
