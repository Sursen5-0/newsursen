using Domain.Models;
using Infrastructure.Persistance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Mappers
{
    public static class EmployeeContractMapper
    {
        public static EmployeeContract ToEntity(this EmployeeContractDTO contract)
        {
            return new EmployeeContract
            {
                Id = Guid.NewGuid(),
                EmployeeId = contract.EmployeeId,
                ExpectedHours = contract.ExpectedHours,
                FromDate = DateOnly.FromDateTime(contract.FromDate),
                ToDate = contract.ToDate == null ? null : DateOnly.FromDateTime(contract.ToDate.Value),
                Title = contract.Title,
                SeveraId = contract.SeveraId
            };
        }
    }
}
