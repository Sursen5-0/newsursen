using Domain.Models;
using Infrastructure.Severa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Severa.Mappers
{
    public static class WorkContractMapper
    {
        public static EmployeeContractDTO ToDto(this SeveraWorkContract contract, Guid employeeId)
        {
            return new EmployeeContractDTO
            {
                Id = default,
                SeveraId = contract.Id,
                EmployeeId = employeeId,
                ExpectedHours = contract.DailyHours * contract.WorkDays?.Length ?? 5,
                FromDate = contract.StartDate,
                ToDate = contract.EndDate,
                Title = contract.Title
            };
        }
    }
}
