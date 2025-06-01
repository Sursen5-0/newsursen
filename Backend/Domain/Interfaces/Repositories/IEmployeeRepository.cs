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
        public Task<IEnumerable<AbsenceDTO>> GetAbsenceByExternalIDs(IEnumerable<Guid> ids);
        public Task InsertEmployeeContracts(IEnumerable<EmployeeContractDTO> employeeDTO);
        public Task UpdateSeveraIds(IEnumerable<SeveraEmployeeModel> employeeDTO);
        public Task UpdateAbsences(IEnumerable<AbsenceDTO> absences);
        public Task InsertAbsences(IEnumerable<AbsenceDTO> absences);

        Task<List<EmployeeDTO>> GetByEntraIdsAsync(IEnumerable<Guid> entraIds);
        Task InsertEmployeesAsync(IEnumerable<EmployeeDTO> dtos);
        Task UpdateEmployeesAsync(IEnumerable<EmployeeDTO> dtos);


    }
}
