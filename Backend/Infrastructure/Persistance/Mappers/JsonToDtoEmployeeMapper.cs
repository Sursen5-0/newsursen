using System;
using System.Linq;
using Domain.Models;
using Infrastructure.Persistance.Models;

namespace Infrastructure.Persistance.Mappers
{
    internal static class JsonToDtoEmployeeMapper
    {
        public static EmployeeDTO ToDto(this EntraEntityModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            return new EmployeeDTO
            {
                EntraId = Guid.TryParse(model.Id, out var g) ? g : Guid.Empty,
                FirstName = model.FirstName ?? string.Empty,
                LastName = model.LastName ?? string.Empty,
                Email = model.Email ?? string.Empty,

                LeaveDate = model.LeaveDate.HasValue
                            ? DateOnly.FromDateTime(model.LeaveDate.Value)
                            : (DateOnly?)null,

                WorkPhoneNumber = model.BusinessPhones?.FirstOrDefault(), 
                PersonalPhoneNumber = model.PersonalPhoneNumber,

                BusinessUnitId = Guid.Empty,
                SeveraId = null,
                FlowCaseId = null
            };
        }
    }
}