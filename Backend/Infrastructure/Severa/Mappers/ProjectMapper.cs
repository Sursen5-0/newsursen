using Domain.Models;
using Infrastructure.Severa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Severa.Mappers
{
    public static class ProjectMapper
    {
        public static ProjectDTO ToDto(this SeveraProjectModel model)
        {
            return new ProjectDTO()
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                Description = model.Description,
                ExternalOwnerId = model.OwnerId,
                ExternalId = model.ExternalId,
                IsClosed = model.IsClosed,                
            };
        }
    }
}
