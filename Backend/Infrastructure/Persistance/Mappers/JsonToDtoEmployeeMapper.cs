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
                // DB-generated Id left as default (Guid.Empty until saved)
                EntraId = Guid.TryParse(model.Id, out var g) ? g : Guid.Empty,
                FirstName = model.FirstName ?? string.Empty,
                LastName = model.LastName ?? string.Empty,
                Email = model.Email ?? string.Empty,

                // Provide DateOnly.MinValue as safe default for missing dates
                HireDate = model.HireDate.HasValue
                                         ? DateOnly.FromDateTime(model.HireDate.Value)
                                         : DateOnly.MinValue,

                LeaveDate = model.LeaveDate.HasValue
                                         ? DateOnly.FromDateTime(model.LeaveDate.Value)
                                         : DateOnly.MinValue,

                // Map first business phone or default empty string
                WorkPhoneNumber = model.BusinessPhones?.FirstOrDefault() ?? string.Empty,

                PersonalPhoneNumber = model.PersonalPhoneNumber ?? string.Empty,

                // Custom-only fields: choose safe defaults or maintain previous values
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