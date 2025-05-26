using Domain.Models;
using Infrastructure.Persistance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Mappers
{
    public static class AbsenceMapper
    {
        public static AbsenceDTO ToDto(this Absence model)
        {
            return new AbsenceDTO
            {
                Id = model.Id,
                EmployeeId = model.EmployeeId,
                ExternalId = model.ExternalId,
                FromDate = model.StartDate,
                ToDate = model.EndDate,
                Type = model.Type
            };
        }
        public static Absence ToEntity(this AbsenceDTO model)
        {
            return new Absence
            {
                Id = model.Id,
                EmployeeId = model.EmployeeId ?? throw new ArgumentNullException("EmployeeID is null while mapping to entity"),
                ExternalId = model.ExternalId,
                StartDate = model.FromDate,
                EndDate = model.ToDate,
                Type = model.Type,
                
            };
        }
    }
}
