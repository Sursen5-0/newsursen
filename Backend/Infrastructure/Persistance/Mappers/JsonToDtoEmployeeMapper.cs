using System;
using System.Linq;
using Domain.Models;
using Infrastructure.Persistance.Models;

namespace Infrastructure.Persistance.Mappers
{
    internal static class JsonToDtoEmployeeMapper
    {
        public static EmployeeDTO ToDto(this EntraEmployeeModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            return new EmployeeDTO
            {
                EntraId = Guid.TryParse(model.Id, out var g) ? g : Guid.Empty,
                FirstName = model.FirstName ?? string.Empty,
                LastName = model.LastName ?? string.Empty,
                Email = model.Email ?? string.Empty,

                HireDate = model.HireDate.HasValue
                                         ? DateOnly.FromDateTime(model.HireDate.Value)
                                         : DateOnly.MinValue,

                LeaveDate = model.LeaveDate.HasValue
                                         ? DateOnly.FromDateTime(model.LeaveDate.Value)
                                         : DateOnly.MinValue,

                WorkPhoneNumber = model.BusinessPhones?.FirstOrDefault() ?? string.Empty,

                PersonalPhoneNumber = model.PersonalPhoneNumber ?? string.Empty,

                Birthdate = DateOnly.MinValue,
                BusinessUnitId = Guid.Empty,
                HubSpotId = string.Empty,
                SeveraId = null,
                ManagerId = null,
                FlowCaseId = string.Empty
            };
        }
    }
}