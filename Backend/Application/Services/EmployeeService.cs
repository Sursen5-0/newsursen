using Domain.Interfaces.ExternalClients;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class EmployeeService(ISeveraClient _severaClient, IEntraClient _entraClient, IFlowCaseClient _flowcaseClient, IEmployeeRepository _employeeRepository, ISkillRepository _skillRepository, IProjectRepository _projectRepository, ILogger<EmployeeService> _logger) : IEmployeeService
    {
        public async Task SynchronizeAbsence()
        {
            _logger.LogInformation($"Start synchronizing of absence");
            var absences = await _severaClient.GetAbsence();
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
                if(result == null)
                {
                    _logger.LogWarning("Unable to find contract for user {Email}",employee.Email);
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
            var phases = await _severaClient.GetPhases(dbExternalProjectIds);

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
            var projects = await _severaClient.GetProjects();
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
                _logger.LogInformation("Synchronizing {Number} on {Count} employees",i, dbEmployees.Length);
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

            var incomingIds = validDtos.Select(d => d.EntraId).ToList();
            var existingDtos = await _employeeRepository.GetByEntraIdsAsync(incomingIds);
            var existingIds = new HashSet<Guid>(existingDtos.Select(e => e.EntraId));

            var insertList = validDtos.Where(d => !existingIds.Contains(d.EntraId)).ToList();
            var updateList = validDtos.Where(d => existingIds.Contains(d.EntraId)).ToList();

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
                    _logger.LogWarning($"Employee {employee.Name}({employee.UserId} has an empty CV ID, skipping.");
                    continue;
                }
                // Check if the employee already exists in the database
                if (!existingEmployees.Any(f => f.FlowCaseId == employee.UserId && f.CvId == employee.DefaultCvId))
                {
                    // Update existing employee
                    var existingEmployee = existingEmployees.First(e => e.Email == employee.Email || e.PrincipalEmail == employee.Email);
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

            var updatedSkillsList = new List<EmployeeSkillDTO>(); // List to hold skills to update
            var newSkillsList = new List<EmployeeSkillDTO>(); // List to hold new skills to insert

            foreach (var employee in employees) // Iterate over each employee
            {
                var skills = await _flowcaseClient.GetSkillsFromCVAsync(employee.FlowCaseId, employee.CvId); // Fetch skills from Flowcase CV
                if (skills == null || !skills.Any()) // If no skills found, skip this employee
                {
                    _logger.LogWarning($"No skills found for employee {employee.Id}, skipping mapping.");
                    continue;
                }
                foreach (var skill in skills) // Iterate over each skill from Flowcase
                {
                    if (!flowCaseSkills.Any(s => s.Id == skill.Id)) // If skill does not exist in repository
                    {
                        _logger.LogInformation($"Adding skill {skill.SkillName} to employee {employee.Id}"); // Log new skill addition
                        newSkillsList.Add(new EmployeeSkillDTO // Add new skill to insert list
                        {
                            Id = employee.Id,
                            SkillId = flowCaseSkills.First(s => s.Id == skill.Id).Id, // Set SkillId from repository
                            YearsOfExperience = skill.SkillTotalDurationInYears // Set years of experience
                        });

                    }
                    else
                    {
                        _logger.LogInformation($"Skill {skill.SkillName} already exists for employee {employee.Id}, skipping addition."); // Log if skill already exists
                    }
                }
            }
            if (newSkillsList.Any()) // If there are new skills to insert
            {
                _logger.LogInformation($"Inserting {newSkillsList.Count} new skills for employees."); // Log insertion
                await _employeeRepository.InsertEmployeeSkills(newSkillsList); // Insert new skills
            }
            else if (updatedSkillsList.Any()) // If there are skills to update
            {
                _logger.LogInformation($"Updating {updatedSkillsList.Count} existing skills for employees."); // Log update
                await _employeeRepository.UpdateEmployeeSkills(updatedSkillsList); // Update skills
            }
            else
            {
                _logger.LogInformation("No skills to update for employees."); // Log if nothing to update
            }
        }
    }
}
