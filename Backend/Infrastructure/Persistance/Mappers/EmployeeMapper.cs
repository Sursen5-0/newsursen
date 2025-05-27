using System;
using Domain.Models;
using Infrastructure.Persistance.Models;

namespace Infrastructure.Persistance.Mappers
{
    public static class EmployeeMapper
    {
        public static Employee ToEntity(this EmployeeDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new Employee
            {
                Id = Guid.NewGuid(),
                EntraId = dto.EntraId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                HireDate = dto.HireDate,
                LeaveDate = dto.LeaveDate,
                Birthdate = dto.Birthdate,
                BusinessUnitId = dto.BusinessUnitId,
                WorkPhoneNumber = dto.WorkPhoneNumber,
                PersonalPhoneNumber = dto.PersonalPhoneNumber,
                HubSpotId = dto.HubSpotId,
                SeveraId = dto.SeveraId,
                ManagerId = dto.ManagerId,
                FlowCaseId = dto.FlowCaseId,
            };
        }

        public static EmployeeDTO ToDto(this Employee entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            return new EmployeeDTO
            {
                Id = entity.Id,
                EntraId = entity.EntraId,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                Email = entity.Email,
                HireDate = entity.HireDate,
                LeaveDate = entity.LeaveDate,
                Birthdate = entity.Birthdate,
                BusinessUnitId = entity.BusinessUnitId,
                WorkPhoneNumber = entity.WorkPhoneNumber,
                PersonalPhoneNumber = entity.PersonalPhoneNumber,
                HubSpotId = entity.HubSpotId,
                SeveraId = entity.SeveraId,
                ManagerId = entity.ManagerId,
                FlowCaseId = entity.FlowCaseId
            };
        }
    }
}
