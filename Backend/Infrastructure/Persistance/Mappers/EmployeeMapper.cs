using Domain.Models;
using Infrastructure.Persistance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Mappers
{
    public static class EmployeeMapper
    {
        public static EmployeeDTO ToDto(this Employee employee)
        {
            return new EmployeeDTO
            {
                Id = employee.Id,
            };
        }
    }
}
