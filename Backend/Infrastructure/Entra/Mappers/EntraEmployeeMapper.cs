using System;
using Domain.Models;
using Infrastructure.Persistance.Models;

namespace Infrastructure.Entra.Mappers
{
    public static class EntraEmployeeMapper
    {
        public static Employee ToEntity(this EmployeeDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new Employee
            {
                Id = dto.Id == default ? Guid.NewGuid() : dto.Id,
                EntraId = dto.EntraId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                HireDate = dto.HireDate,
                LeaveDate = dto.LeaveDate,
                WorkPhoneNumber = dto.WorkPhoneNumber,
                PersonalPhoneNumber = dto.PersonalPhoneNumber,
                SeveraId = dto.SeveraId,
                ManagerId = dto.ManagerId,
                UserPrincipalName = dto.UserPrincipalName,
                FlowCaseId = dto.FlowCaseId,
                FlowCaseCVId = dto.CvId,
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
                LeaveDate = entity.LeaveDate,
                WorkPhoneNumber = entity.WorkPhoneNumber,
                PersonalPhoneNumber = entity.PersonalPhoneNumber,
                FlowCaseId = entity.FlowCaseId,
                CvId = entity.FlowCaseCVId,
                SeveraId = entity.SeveraId,
                ManagerId = entity.ManagerId,
                UserPrincipalName = entity.UserPrincipalName
            };
        }
    }
}
