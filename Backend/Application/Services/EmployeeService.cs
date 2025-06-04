using Domain.Interfaces.ExternalClients;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class EmployeeService(ISeveraClient _severaClient, IEntraClient _entraClient, IFlowCaseClient _flowcaseClient,
        IEmployeeRepository _employeeRepository, ISkillRepository _skillRepository, IProjectRepository _projectRepository,
        ILogger<EmployeeService> _logger, IJobExecutionRepository _jobExecutionRepo) : IEmployeeService
    {
        public async Task SynchronizeAbsence()
        {
            _logger.LogInformation($"Start synchronizing of absence");
            var latestSuccessfullUpdate = await _jobExecutionRepo.GetLatestSuccessfulJobExecutionByName(nameof(SynchronizeAbsence));
            var absences = await _severaClient.GetAbsence(latestSuccessfullUpdate);
            if (absences == null || !absences.Any())
            {
                _logger.LogWarning($"_severaClient.GetAbsence returned empty, not continuing syncronization of absence");
                return;
            }
            var employees = await _employeeRepository.GetEmployees();
            var dbAbsences = await _employeeRepository.GetAbsenceByExternalIDs(absences.Select(x => x.ExternalId));
            var dbExternalIds = new HashSet<Guid>(dbAbsences.Select(x => x.ExternalId));

            var absenceArray = absences.ToArray();
            for (int i = 0; i < absences.Count(); i++)
            {
                var employee = employees.FirstOrDefault(x => x.SeveraId == absenceArray[i].SeveraEmployeeId);
                if (employee == null)
                {
                    _logger.LogWarning("No employeeId found for the severa user with ID: {absence.ExternalId}", absenceArray[i].ExternalId);
                    continue;
                }
                absenceArray[i].EmployeeId = employee.Id;
            }


            var updateList = new List<AbsenceDTO>();
            var insertList = new List<AbsenceDTO>();
            foreach (var item in absenceArray)
            {
                if (dbExternalIds.Any(x => x == item.ExternalId))
                {
                    updateList.Add(item);
                }
                else
                {
                    insertList.Add(item);
                }
            }

            insertList = insertList.Where(x => x.EmployeeId.HasValue).ToList();
            _logger.LogInformation("Updating {updateamount}", updateList.Count);
            await _employeeRepository.UpdateAbsences(updateList);
            _logger.LogInformation("Inserting {insertamount}", insertList.Count);
            await _employeeRepository.InsertAbsences(insertList);
            _logger.LogInformation($"Done synchronizing of absence");

        }

        public async Task SynchronizeContracts()
        {
            _logger.LogInformation($"Start synchronizing of contracts");

            var employees = await _employeeRepository.GetEmployees();
            employees = employees.Where(x => x.SeveraId != null);
            var contracts = new List<EmployeeContractDTO>();
            _logger.LogInformation($"Synchronizing on {employees.Count()} employees contracts");

            foreach (var employee in employees)
            {
                var result = await _severaClient.GetWorkContractByUserId(employee.SeveraId!.Value);
                if (result == null)
                {
                    _logger.LogWarning("Unable to find contract for user {Email}", employee.Email);
                    continue;
                }
                result.EmployeeId = employee.Id;
                contracts.Add(result);
            }

            _logger.LogInformation($"Done pulling data from Severa, starting insert to db");
            await _employeeRepository.InsertEmployeeContracts(contracts);

            _logger.LogInformation($"Done synchronizing contracts");
        }

        public async Task SynchronizePhases()
        {
            _logger.LogInformation($"Start synchronizing of project phases");
            var dbPhases = await _projectRepository.GetPhases();
            var projects = await _projectRepository.GetProjects();
            var dbExternalProjectIds = new HashSet<Guid>(projects.Select(x => x.ExternalId));
            var dbExternalPhaseIds = new HashSet<Guid>(dbPhases.Select(x => x.ExternalId));
            var latestSuccessfullUpdate = await _jobExecutionRepo.GetLatestSuccessfulJobExecutionByName(nameof(SynchronizePhases));

            var phases = await _severaClient.GetPhases(dbExternalProjectIds, latestSuccessfullUpdate);

            var phaseArray = phases.ToArray();
            _logger.LogInformation("Synchronizing on {0} projects phases", phaseArray.Length);

            for (int i = 0; i < phaseArray.Count(); i++)
            {
                var project = projects.FirstOrDefault(x => x.ExternalId == phaseArray[i].ExternalProjectId);
                if (project == null)
                {
                    _logger.LogError($"No project found for the phase user with external ID: {phaseArray[i].ExternalId}");
                    continue;
                }
                phaseArray[i].ProjectId = project.Id;
            }


            var updateList = phaseArray.Where(x => dbExternalPhaseIds.Contains(x.ExternalId)).ToList();
            var insertList = phaseArray.Where(x => !dbExternalPhaseIds.Contains(x.ExternalId)).ToList();

            _logger.LogInformation($"Done pulling data from Severa, starting insert to db");
            await _projectRepository.InsertPhases(insertList);
            _logger.LogInformation($"Done insert to db, starting update existing items");
            await _projectRepository.UpdatePhases(updateList);

            _logger.LogInformation($"Done synchronizing phases");

        }

        public async Task SynchronizeProjects()
        {
            _logger.LogInformation($"Start synchronizing of projects");
            var dbProjects = await _projectRepository.GetProjects();
            var dbExternalIds = new HashSet<Guid>(dbProjects.Select(x => x.ExternalId));
            var employees = await _employeeRepository.GetEmployees();

            _logger.LogInformation($"Getting data from Severa...");
            var latestSuccessfullUpdate = await _jobExecutionRepo.GetLatestSuccessfulJobExecutionByName(nameof(SynchronizeProjects));

            var projects = await _severaClient.GetProjects(latestSuccessfullUpdate);
            _logger.LogInformation("Synchronizing on {projects.Count()} projects", projects.Count());
            var projectArray = projects.ToArray();

            for (int i = 0; i < projectArray.Count(); i++)
            {
                var employee = employees.FirstOrDefault(x => x.SeveraId == projectArray[i].ExternalOwnerId);
                if (employee == null)
                {
                    _logger.LogWarning("No employeeId found for the severa user with ID: {absence.ExternalId}", projectArray[i].ExternalId);
                    continue;
                }
                projectArray[i].OwnerId = employee.Id;
            }


            var updateList = projectArray.Where(a => dbExternalIds.Contains(a.ExternalId)).ToList();
            var insertList = projectArray.Where(a => !dbExternalIds.Contains(a.ExternalId)).ToList();

            _logger.LogInformation($"Done pulling data from Severa, starting insert to db");
            await _projectRepository.InsertProjects(insertList);
            _logger.LogInformation($"Done insert to db, starting update existing items");
            await _projectRepository.UpdateProjects(updateList);

            _logger.LogInformation($"Done synchronizing Projects");

        }

        public async Task SynchronizeUnmappedSeveraIds()
        {
            _logger.LogInformation($"Start synchronizing of severaIds");
            var dbEmployees = (await _employeeRepository.GetEmployeeWithoutSeveraIds()).ToArray();
            var severaEmployees = new List<SeveraEmployeeModel>();

            _logger.LogInformation("Synchronizing on {Count} employees", dbEmployees.Length);
            for (int i = 0; i < dbEmployees.Length; i++)
            {
                _logger.LogInformation("Synchronizing {Number} on {Count} employees", i, dbEmployees.Length);
                var data = await _severaClient.GetUserByEmail(dbEmployees[i].Email);
                if (data == null)
                {
                    _logger.LogWarning("Unable to find user in Severa with email: {Email}", dbEmployees[i].Email);
                    continue;
                }
                severaEmployees.Add(data);
            }

            await _employeeRepository.UpdateSeveraIds(severaEmployees);
            _logger.LogInformation($"Done synchronizing of severaIds");
        }

        public async Task SynchronizeEmployeesAsync()
        {
            var dtos = await _entraClient.GetAllEmployeesAsync() ?? new List<EmployeeDTO>();

            var validDtos = new List<EmployeeDTO>();
            foreach (var dto in dtos)
            {
                if (dto == null)
                {
                    _logger.LogWarning("Skipped null EmployeeDTO");
                    continue;
                }
                validDtos.Add(dto);
            }

            if (!validDtos.Any())
            {
                _logger.LogInformation("No valid employees to synchronize.");
                return;
            }

            var existingEmployees = (await _employeeRepository.GetEmployees()).ToList();
            var allEmployees = new List<EmployeeDTO>(existingEmployees);
            allEmployees.AddRange(dtos.Where(x => !existingEmployees.Select(x => x.EntraId).Contains(x.EntraId)));
            var mapEmployees = allEmployees.ToDictionary(x => x.EntraId);
            var existingIds = new HashSet<Guid>(existingEmployees.Select(e => e.EntraId));

            var dbMap = existingEmployees.ToDictionary(e => e.EntraId, e => e.Id);
            var employeeArray = validDtos.ToArray();



            var insertList = employeeArray.Where(d => !existingIds.Contains(d.EntraId)).ToList();
            insertList.ForEach(x => x.Id = Guid.NewGuid());
            var updateList = employeeArray.Where(d => existingIds.Contains(d.EntraId)).ToList();
            SetManagerIds(insertList, existingEmployees);
            SetManagerIds(updateList, existingEmployees);
            if (insertList.Any())
            {
                await _employeeRepository.InsertEmployeesAsync(insertList);
                foreach (var dto in insertList)
                    _logger.LogInformation("Inserted employee {EntraId}", dto.EntraId);
            }
            if (updateList.Any())
            {
                await _employeeRepository.UpdateEmployeesAsync(updateList);
                foreach (var dto in updateList)
                    _logger.LogInformation("Updated employee {EntraId}", dto.EntraId);
            }
            _logger.LogInformation("Done SynchronizeEmployeesAsync from entra");

        }
        private void SetManagerIds(List<EmployeeDTO> dtos, IEnumerable<EmployeeDTO> existingEmployees)
        {
            var allEmployees = new List<EmployeeDTO>(existingEmployees);
            allEmployees.AddRange(dtos.Where(x => !existingEmployees.Select(x => x.EntraId).Contains(x.EntraId)));
            var mapEmployees = allEmployees.ToDictionary(x => x.EntraId);
            var existingIds = new HashSet<Guid>(existingEmployees.Select(e => e.EntraId));

            var dbMap = existingEmployees.ToDictionary(e => e.EntraId, e => e.Id);
            var dtoArray = dtos.ToArray();
            for (int i = 0; i < dtoArray.Length; i++)
            {
                if (!dtoArray[i].EntraManagerId.HasValue)
                {
                    _logger.LogWarning("No manager found for user {Name}", dtoArray[i].FirstName + " " + dtoArray[i].LastName);
                    continue;
                }
                mapEmployees.TryGetValue(dtoArray[i].EntraManagerId.Value, out var manager);
                if (manager == null || manager.Id == default)
                {
                    _logger.LogWarning("No manager found for user {Name}", dtoArray[i].FirstName + " " + dtoArray[i].LastName);
                    continue;
                }
                dtoArray[i].ManagerId = manager.Id;
            }
        }

        public async Task SynchronizeEmployeesWithFlowcaseIdsAsync()
        {
            _logger.LogInformation("Synchronizing employees with Flowcase IDs and CV IDs");
            var employees = await _flowcaseClient.GetUsersAsync();
            if (employees == null || !employees.Any())
            {
                _logger.LogWarning("No employees found in Flowcase, skipping synchronization.");
                return;
            }
            var existingEmployees = await _employeeRepository.GetEmployees();

            var updateList = new List<EmployeeDTO>();
            foreach (var employee in employees)
            {
                if (employee.UserId == null)
                {
                    _logger.LogWarning($"Employee {employee.Name} has an has an empty FlowCase ID, skipping.");
                    continue;
                }
                if (employee.DefaultCvId == null)
                {
                    _logger.LogWarning($"Employee {employee.Name}({employee.UserId}) has an empty CV ID, skipping.");
                    continue;
                }
                // Check if the employee already exists in the database
                if (!existingEmployees.Any(f => f.FlowCaseId == employee.UserId && f.CvId == employee.DefaultCvId))
                {
                    // Update existing employee
                    var existingEmployee = existingEmployees.FirstOrDefault(e => e.Email == employee.Email || e.UserPrincipalName == employee.Email);
                    if (existingEmployee == null)
                    {
                        _logger.LogWarning($"No existing employee found for {employee.Name} with email {employee.Email}, skipping.");
                        continue;
                    }
                    existingEmployee.FlowCaseId = employee.UserId;
                    existingEmployee.CvId = employee.DefaultCvId;
                    updateList.Add(existingEmployee);
                }
            }

            if (updateList.Any())
            {
                await _employeeRepository.UpdateEmployeesAsync(updateList);
            }
        }

        public async Task SynchronizeEmployeeSkillsAsync()
        {
            _logger.LogInformation("Mapping employee skills"); // Log start of skill mapping
            var employees = await _employeeRepository.GetEmployees(); // Fetch all employees from repository
            var flowCaseSkills = await _skillRepository.GetAllSkillsAsync(); // Fetch all skills from repository

            var tempSkillList = new List<EmployeeSkillDTO>(); // List to hold new skills to insert

            if (!flowCaseSkills.Any())
            {
                _logger.LogWarning("Skills table is empty, cannot synchronize employee skills");
                return;
            }

            var usableEmployees = employees.Where(e => e.FlowCaseId != null && e.CvId != null).ToList(); // Filter employees with valid FlowCaseId and CvId
            if (!usableEmployees.Any())
            {
                _logger.LogWarning("Employees have not been synched with flowcase ID's, skipping job");
                return;
            }

            foreach (var employee in usableEmployees) // Iterate over each employee
            {
                var skills = await _flowcaseClient.GetSkillsFromCVAsync(employee.FlowCaseId, employee.CvId); // Fetch skills from Flowcase CV
                if (skills == null || !skills.Any()) // If no skills found, skip this employee
                {
                    _logger.LogWarning($"No skills found for employee {employee.Id}, skipping mapping.");
                    continue;
                }
                var employeeSkills = skills.Select(x => new EmployeeSkillDTO()
                {
                    EmployeeId = employee.Id,
                    SkillId = flowCaseSkills.FirstOrDefault(s => s.ExternalId == x.ExternalId)?.Id,
                    Name = x.Name,
                    ExternalId = x.ExternalId,
                    YearsOfExperience = x.YearsOfExperience
                });
                tempSkillList.AddRange(employeeSkills);
            }

            var allEmployeeSkills = await _employeeRepository.GetEmployeeSkills();
            
            var updatedSkillsList = tempSkillList.Where(flowcaseSkill => allEmployeeSkills.Any(dbskill => dbskill.ExternalId == flowcaseSkill.ExternalId && dbskill.EmployeeId == flowcaseSkill.EmployeeId));
            var newSkillList = tempSkillList.Except(updatedSkillsList);
            
            var updateListIds = updatedSkillsList.Select(x => new { x.EmployeeId, x.ExternalId }).ToHashSet();
            var deleteSkillList = allEmployeeSkills.Where(x => !updateListIds.Contains(new { x.EmployeeId, x.ExternalId }));

            if (newSkillList.Any()) // If there are new skills to insert
            {
                _logger.LogInformation($"Inserting {newSkillList.Count()} new skills for employees."); // Log insertion
                await _employeeRepository.InsertEmployeeSkills(newSkillList); // Insert new skills
            }
            if (updatedSkillsList.Any()) // If there are skills to update
            {
                _logger.LogInformation($"Updating {updatedSkillsList.Count()} existing skills for employees."); // Log update
                await _employeeRepository.UpdateEmployeeSkills(updatedSkillsList); // Update skills
            }
            if (deleteSkillList.Any()) // If there are skills to delete
            {
                _logger.LogInformation($"Deleting {deleteSkillList.Count()} existing skills for employees."); // Log update
                await _employeeRepository.DeleteEmployeeSkills(deleteSkillList.Select(x=> x.Id)); // Update skills
            }
        }
    }
}
