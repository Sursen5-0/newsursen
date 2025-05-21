using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Repositories
{
    public interface IEmployeeRepository
    {
        public Task<IEnumerable<EmployeeDTO>> GetEmployees(bool includeDisabled = false);
        public Task<IEnumerable<EmployeeDTO>> GetEmployeeWithoutSeveraIds(bool includeDisabled = false);
        public Task InsertEmployeeContracts(IEnumerable<EmployeeContractDTO> employeeDTO);
        public Task UpdateSeveraIds(IEnumerable<SeveraEmployeeModel> employeeDTO);
    }
}
