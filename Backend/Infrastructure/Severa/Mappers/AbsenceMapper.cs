using Domain.Models;
using Infrastructure.Severa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Severa.Mappers
{
    public static class AbsenceMapper
    {
        public static AbsenceDTO ToDto(this SeveraActivityModel absence)
        {
            return new AbsenceDTO()
            {
                Id = Guid.NewGuid(),
                ExternalId = absence.Id,
                SeveraEmployeeId = absence.SeveraEmployeeId,
                FromDate = absence.FromDate,
                ToDate = absence.ToDate,
                Type = absence.Type
            };
        }

    }
}
