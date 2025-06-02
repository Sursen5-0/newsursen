using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Services
{
    public interface IEmployeeService
    {
        public Task SynchronizeContracts();
        public Task SynchronizeUnmappedSeveraIds();
        public Task SynchronizeAbsence();

        Task SynchronizeEmployeesAsync();
        public Task SynchronizeProjects();
        public Task SynchronizePhases();

        public Task SynchronizeEmployeesWithFlowcaseIdsAsync();

        public Task SynchronizeEmployeeSkillsAsync();
    }
}
