using Application.Services;
using Bogus;
using Domain.Interfaces.ExternalClients;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Infrastructure.Persistance.Models;
using Infrastructure.Persistance.Repositories;
using Infrastructure.Severa;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTests.Common;
namespace UnitTests.Services
{
    public class EmployeeServiceTests
    {
        private readonly Mock<IEmployeeRepository> _employeeRepoMock = new Mock<IEmployeeRepository>();
        private readonly Mock<ISeveraClient> _severaClientMock = new Mock<ISeveraClient>();
        private readonly Mock<IProjectRepository> _projectRepoMock = new Mock<IProjectRepository>();
        private readonly Mock<ILogger<EmployeeService>> _loggerMock = new Mock<ILogger<EmployeeService>>();
        private readonly Mock<IEntraClient> _entraClientMock = new Mock<IEntraClient>();
        private readonly Mock<IJobExecutionRepository> _JobExecutionRepoMock = new Mock<IJobExecutionRepository>();
        private readonly Mock<IFlowCaseClient> _flowcaseClientMock = new Mock<IFlowCaseClient>();
        private readonly Mock<ISkillRepository> _skillRepositoryMock = new Mock<ISkillRepository>();
        private EmployeeService _sut;

        public EmployeeServiceTests()
        {
            _sut = new EmployeeService(_severaClientMock.Object,
                _entraClientMock.Object,
                _flowcaseClientMock.Object,
                _employeeRepoMock.Object,
                _skillRepositoryMock.Object,
                _projectRepoMock.Object,
                _loggerMock.Object,
                _JobExecutionRepoMock.Object);
        }
        [Fact]
        public async Task SynchronizeContracts_WithNoEmployees_DoesntCallGetWorkContractByUserId()
        {
            // Arrange
            var emptyEmployeeList = new List<EmployeeDTO>();
            _employeeRepoMock.Setup(x => x.GetEmployees(It.IsAny<bool>())).ReturnsAsync(emptyEmployeeList);
            _employeeRepoMock.Setup(x => x.InsertEmployeeContracts(It.IsAny<IEnumerable<EmployeeContractDTO>>())).Verifiable();

            _employeeRepoMock.Setup(x => x.GetEmployees(It.IsAny<bool>())).ReturnsAsync(emptyEmployeeList);
            _employeeRepoMock.Setup(x => x.InsertEmployeeContracts(It.IsAny<IEnumerable<EmployeeContractDTO>>())).Verifiable();


            // Act
            await _sut.SynchronizeContracts();

            // Assert
            _severaClientMock.Verify(x => x.GetWorkContractByUserId(It.IsAny<Guid>()), Times.Never);
        }


        [Fact]
        public async Task SynchronizeContracts_WithOneEmployeesWithoutSeveraID_DoesntCallGetWorkContractByUserId()
        {
            // Arrange
            var emptyEmployeeList = new List<EmployeeDTO>() { new EmployeeDTO() { Id = Guid.NewGuid() } };
            _employeeRepoMock.Setup(x => x.GetEmployees(It.IsAny<bool>())).ReturnsAsync(emptyEmployeeList);
            _employeeRepoMock.Setup(x => x.InsertEmployeeContracts(It.IsAny<IEnumerable<EmployeeContractDTO>>())).Verifiable();

            // Act
            await _sut.SynchronizeContracts();

            // Assert
            _severaClientMock.Verify(x => x.GetWorkContractByUserId(It.IsAny<Guid>()), Times.Never);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(1000)]
        public async Task SynchronizeContracts_WithOneEmployeesWithSeveraID_CallGetWorkContractByUserIdOnce(int times)
        {
            // Arrange

            var faker = new Faker<EmployeeDTO>()
                .RuleFor(x => x.Id, x => Guid.NewGuid())
                .RuleFor(x => x.SeveraId, x => Guid.NewGuid());

            _employeeRepoMock.Setup(x => x.GetEmployees(It.IsAny<bool>())).ReturnsAsync(faker.Generate(times));
            _employeeRepoMock.Setup(x => x.InsertEmployeeContracts(It.IsAny<IEnumerable<EmployeeContractDTO>>())).Verifiable();
            _severaClientMock.Setup(x => x.GetWorkContractByUserId(It.IsAny<Guid>())).ReturnsAsync(new EmployeeContractDTO());

            // Act
            await _sut.SynchronizeContracts();

            // Assert
            _severaClientMock.Verify(x => x.GetWorkContractByUserId(It.IsAny<Guid>()), Times.Exactly(times));
        }

        [Fact]
        public async Task SynchronizeUnmappedSeveraIds_WithOneEmployeesWithoutSeveraID_DoesntCallGetUserByEmail()
        {
            // Arrange
            var faker = new Faker<EmployeeDTO>()
                .RuleFor(x => x.Id, x => Guid.NewGuid())
                .RuleFor(x => x.SeveraId, x => null);

            _employeeRepoMock.Setup(x => x.GetEmployeeWithoutSeveraIds(It.IsAny<bool>())).ReturnsAsync(faker.Generate(1));
            _severaClientMock.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(new SeveraEmployeeModel());
            _employeeRepoMock.Setup(x => x.UpdateSeveraIds(It.IsAny<List<SeveraEmployeeModel>>())).Verifiable();

            // Act
            await _sut.SynchronizeContracts();

            // Assert
            _severaClientMock.Verify(x => x.GetUserByEmail(It.IsAny<string>()), Times.Never);
        }
        [Fact]
        public async Task SynchronizeProjects_InsertsAndUpdatesCorrectly()
        {
            // Arrange
            var dbProjects = new List<ProjectDTO>
            {
                new ProjectDTO() { ExternalId = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name ="test" }
            };
            var severaProjects = new List<ProjectDTO>
            {
                new ProjectDTO() { ExternalId = Guid.Parse("11111111-1111-1111-1111-111111111111"), ExternalOwnerId = Guid.NewGuid(), Name ="test" },
                new ProjectDTO() { ExternalId = Guid.Parse("22222222-2222-2222-2222-222222222222"), ExternalOwnerId = Guid.NewGuid(), Name ="test" }
            };
            var employees = new List<EmployeeDTO>
            {
                new EmployeeDTO() { SeveraId = Guid.NewGuid(), Id = Guid.NewGuid() },
                new EmployeeDTO() { SeveraId = Guid.NewGuid(), Id = Guid.NewGuid() }
            };

            _projectRepoMock.Setup(x => x.GetProjects(It.IsAny<bool>())).ReturnsAsync(dbProjects);
            _severaClientMock.Setup(x => x.GetProjects(It.IsAny<DateTime?>(), It.IsAny<int?>())).ReturnsAsync(severaProjects);
            _employeeRepoMock.Setup(x => x.GetEmployees(It.IsAny<bool>())).ReturnsAsync(employees);

            // Act
            await _sut.SynchronizeProjects();

            // Assert
            _projectRepoMock.Verify(x => x.InsertProjects(
                It.Is<List<ProjectDTO>>(x => x.Count == 1 && x.First().ExternalId == Guid.Parse("22222222-2222-2222-2222-222222222222"))), Times.Once);

            _projectRepoMock.Verify(x => x.UpdateProjects(
                It.Is<List<ProjectDTO>>(x => x.Count == 1 && x.First().ExternalId == Guid.Parse("11111111-1111-1111-1111-111111111111"))), Times.Once);
        }

        [Fact]
        public async Task SynchronizeProjects_LogsError_WhenEmployeeNotFound()
        {
            // Arrange
            var severaProjects = new List<ProjectDTO>
            {
                new ProjectDTO() { ExternalId = Guid.NewGuid(), ExternalOwnerId = Guid.NewGuid() , Name ="test"}
            };
            _projectRepoMock.Setup(x => x.GetProjects(It.IsAny<bool>())).ReturnsAsync(new List<ProjectDTO>());
            _severaClientMock.Setup(x => x.GetProjects(It.IsAny<DateTime?>(), It.IsAny<int?>())).ReturnsAsync(severaProjects);
            _employeeRepoMock.Setup(x => x.GetEmployees(It.IsAny<bool>())).ReturnsAsync(new List<EmployeeDTO>());

            // Act
            await _sut.SynchronizeProjects();

            // Assert
            _projectRepoMock.Verify(x => x.InsertProjects(It.Is<List<ProjectDTO>>(x => x.Count == 1)), Times.Once);
            _loggerMock.VerifyLog(LogLevel.Warning, Times.Once());
        }

        [Fact]
        public async Task SynchronizeProjects_AllAreInserts_WhenDbIsEmpty()
        {
            // Arrange
            var severaProjects = new List<ProjectDTO>
            {
                new ProjectDTO() { ExternalId = Guid.NewGuid(), ExternalOwnerId = Guid.NewGuid(), Name ="test" }
            };
            var employees = new List<EmployeeDTO>
            {
                new EmployeeDTO() { SeveraId = Guid.NewGuid(), Id = Guid.NewGuid() , }
            };

            _projectRepoMock.Setup(x => x.GetProjects(It.IsAny<bool>())).ReturnsAsync(new List<ProjectDTO>());
            _severaClientMock.Setup(x => x.GetProjects(It.IsAny<DateTime?>(), It.IsAny<int?>())).ReturnsAsync(severaProjects);
            _employeeRepoMock.Setup(x => x.GetEmployees(It.IsAny<bool>())).ReturnsAsync(employees);

            // Act
            await _sut.SynchronizeProjects();

            // Assert
            _projectRepoMock.Verify(x => x.InsertProjects(It.Is<List<ProjectDTO>>(x => x.Count == 1)), Times.Once);
            _projectRepoMock.Verify(x => x.UpdateProjects(It.Is<List<ProjectDTO>>(x => x.Count == 0)), Times.Once);
        }

        [Fact]
        public async Task SynchronizeProjects_AllAreUpdates_WhenAllExistInDb()
        {
            // Arrange
            var externalId = Guid.NewGuid();
            var severaProjects = new List<ProjectDTO>
            {
                new ProjectDTO() { ExternalId = externalId, ExternalOwnerId = Guid.NewGuid(), Name = "Test" }
            };
            var dbProjects = new List<ProjectDTO>
            {
                new ProjectDTO() { ExternalId = externalId, Name = "Test" }
            };
            var employees = new List<EmployeeDTO>
            {
                new EmployeeDTO() { SeveraId = Guid.NewGuid(), Id = Guid.NewGuid() }
            };

            _projectRepoMock.Setup(x => x.GetProjects(It.IsAny<bool>())).ReturnsAsync(dbProjects);
            _severaClientMock.Setup(x => x.GetProjects(It.IsAny<DateTime?>(), It.IsAny<int?>())).ReturnsAsync(severaProjects);
            _employeeRepoMock.Setup(x => x.GetEmployees(It.IsAny<bool>())).ReturnsAsync(employees);

            // Act
            await _sut.SynchronizeProjects();

            // Assert
            _projectRepoMock.Verify(x => x.InsertProjects(It.Is<List<ProjectDTO>>(x => x.Count == 0)), Times.Once);
            _projectRepoMock.Verify(x => x.UpdateProjects(It.Is<List<ProjectDTO>>(x => x.Count == 1)), Times.Once);
        }

        [Fact]
        public async Task SynchronizeEmployeesWithFlowcaseIdsAsync_UpdatesEmployees_WhenFlowcaseIdsMissing()
        {
            // Arrange
            var flowcaseUsers = new List<FlowcaseUserModel>
            {
                new FlowcaseUserModel { UserId = "fc1", DefaultCvId = "cv1", Name = "Test User", Email = "test@example.com" }
            };
            var existingEmployees = new List<EmployeeDTO>
            {
                new EmployeeDTO { Id = Guid.NewGuid(), Email = "test@example.com", FlowCaseId = null, CvId = null }
            };

            _flowcaseClientMock.Setup(x => x.GetUsersAsync()).ReturnsAsync(flowcaseUsers);
            _employeeRepoMock.Setup(x => x.GetEmployees(It.IsAny<bool>())).ReturnsAsync(existingEmployees);
            _employeeRepoMock.Setup(x => x.UpdateEmployeesAsync(It.IsAny<IEnumerable<EmployeeDTO>>())).Returns(Task.CompletedTask).Verifiable();

            // Act
            await _sut.SynchronizeEmployeesWithFlowcaseIdsAsync();

            // Assert
            _employeeRepoMock.Verify(x => x.UpdateEmployeesAsync(It.Is<IEnumerable<EmployeeDTO>>(list =>
                list.Any(e => e.FlowCaseId == "fc1" && e.CvId == "cv1"))), Times.Once);
        }











        [Fact]
        public async Task SynchronizeEmployeesAsync_WithNoEmployeesFromEntra_DoesNotCallRepo()
        {
            // Arrange
            _entraClientMock
                .Setup(x => x.GetAllEmployeesAsync())
                .ReturnsAsync((List<EmployeeDTO>)null);

            // Act
            await _sut.SynchronizeEmployeesAsync();

            // Assert
            _employeeRepoMock.Verify(x => x.GetEmployees(It.IsAny<bool>()), Times.Never);
            _employeeRepoMock.Verify(x => x.InsertEmployeesAsync(It.IsAny<IEnumerable<EmployeeDTO>>()), Times.Never);
            _employeeRepoMock.Verify(x => x.UpdateEmployeesAsync(It.IsAny<IEnumerable<EmployeeDTO>>()), Times.Never);
        }

        [Fact]
        public async Task SynchronizeEmployeesAsync_WithOnlyNullDTOs_SkipsAll()
        {
            // Arrange
            var dtos = new List<EmployeeDTO> { null, null };
            _entraClientMock
                .Setup(x => x.GetAllEmployeesAsync())
                .ReturnsAsync(dtos);

            // Act
            await _sut.SynchronizeEmployeesAsync();

            // Assert
            _employeeRepoMock.Verify(x => x.GetEmployees(It.IsAny<bool>()), Times.Never);
            _employeeRepoMock.Verify(x => x.InsertEmployeesAsync(It.IsAny<IEnumerable<EmployeeDTO>>()), Times.Never);
            _employeeRepoMock.Verify(x => x.UpdateEmployeesAsync(It.IsAny<IEnumerable<EmployeeDTO>>()), Times.Never);
        }

        [Fact]
        public async Task SynchronizeEmployeesAsync_InsertsAndUpdatesCorrectly()
        {
            // Arrange
            var existing1 = new EmployeeDTO { EntraId = Guid.NewGuid(), Id = Guid.NewGuid() };
            var existing2 = new EmployeeDTO { EntraId = Guid.NewGuid(), Id = Guid.NewGuid() };
            _employeeRepoMock
                .Setup(x => x.GetEmployees(It.IsAny<bool>()))
                .ReturnsAsync(new List<EmployeeDTO> { existing1, existing2 });

            var newDto = new EmployeeDTO { EntraId = Guid.NewGuid(), FirstName = "John", LastName = "Doe" };
            var updatedDto = new EmployeeDTO { EntraId = existing1.EntraId, FirstName = "Jane", LastName = "Smith" };
            var dtos = new List<EmployeeDTO> { newDto, updatedDto };
            _entraClientMock
                .Setup(x => x.GetAllEmployeesAsync())
                .ReturnsAsync(dtos);

            List<EmployeeDTO> inserted = null;
            List<EmployeeDTO> updated = null;
            _employeeRepoMock
                .Setup(x => x.InsertEmployeesAsync(It.IsAny<IEnumerable<EmployeeDTO>>()))
                .Callback<IEnumerable<EmployeeDTO>>(list => inserted = list.ToList())
                .Returns(Task.CompletedTask);
            _employeeRepoMock
                .Setup(x => x.UpdateEmployeesAsync(It.IsAny<IEnumerable<EmployeeDTO>>()))
                .Callback<IEnumerable<EmployeeDTO>>(list => updated = list.ToList())
                .Returns(Task.CompletedTask);

            // Act
            await _sut.SynchronizeEmployeesAsync();

            // Assert
            Assert.Single(inserted);
            Assert.Equal(newDto.EntraId, inserted[0].EntraId);
            Assert.Single(updated);
            Assert.Equal(updatedDto.EntraId, updated[0].EntraId);
        }

        [Fact]
        public async Task SynchronizeEmployeesAsync_SetsManagerIdForNewDtos()
        {
            // Arrange
            var manager = new EmployeeDTO { EntraId = Guid.NewGuid(), Id = Guid.NewGuid() };
            _employeeRepoMock
                .Setup(x => x.GetEmployees(It.IsAny<bool>()))
                .ReturnsAsync(new List<EmployeeDTO> { manager });

            var newDto = new EmployeeDTO
            {
                EntraId = Guid.NewGuid(),
                FirstName = "Alice",
                LastName = "Brown",
                EntraManagerId = manager.EntraId
            };
            _entraClientMock
                .Setup(x => x.GetAllEmployeesAsync())
                .ReturnsAsync(new List<EmployeeDTO> { newDto });

            List<EmployeeDTO> inserted = null;
            _employeeRepoMock
                .Setup(x => x.InsertEmployeesAsync(It.IsAny<IEnumerable<EmployeeDTO>>()))
                .Callback<IEnumerable<EmployeeDTO>>(list => inserted = list.ToList())
                .Returns(Task.CompletedTask);

            // Act
            await _sut.SynchronizeEmployeesAsync();

            // Assert
            Assert.Single(inserted);
            Assert.Equal(manager.Id, inserted[0].ManagerId);
        }

        [Fact]
        public async Task SynchronizeEmployeesAsync_WithNullManager_LogsWarning()
        {
            // Arrange
            _employeeRepoMock
                .Setup(x => x.GetEmployees(It.IsAny<bool>()))
                .ReturnsAsync(new List<EmployeeDTO>());

            var dto = new EmployeeDTO
            {
                EntraId = Guid.NewGuid(),
                FirstName = "Bob",
                LastName = "Taylor",
                EntraManagerId = null
            };
            _entraClientMock
                .Setup(x => x.GetAllEmployeesAsync())
                .ReturnsAsync(new List<EmployeeDTO> { dto });

            _employeeRepoMock
                .Setup(x => x.InsertEmployeesAsync(It.IsAny<IEnumerable<EmployeeDTO>>()))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.SynchronizeEmployeesAsync();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"No manager found for user {dto.FirstName} {dto.LastName}"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }



        [Fact]
        public async Task SynchronizeAbsence_WhenGetAbsenceReturnsNull_LogsWarningAndReturns()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _JobExecutionRepoMock
                .Setup(x => x.GetLatestSuccessfulJobExecutionByName(nameof(EmployeeService.SynchronizeAbsence)))
                .ReturnsAsync(now);

            _severaClientMock
                .Setup(x => x.GetAbsence(It.IsAny<DateTime?>(), null))
                .ReturnsAsync((IEnumerable<AbsenceDTO>)null);

            // Act
            await _sut.SynchronizeAbsence();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GetAbsence returned empty")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _employeeRepoMock.Verify(x => x.GetEmployees(It.IsAny<bool>()), Times.Never);
            _employeeRepoMock.Verify(x => x.GetAbsenceByExternalIDs(It.IsAny<IEnumerable<Guid>>()), Times.Never);
            _employeeRepoMock.Verify(x => x.UpdateAbsences(It.IsAny<IEnumerable<AbsenceDTO>>()), Times.Never);
            _employeeRepoMock.Verify(x => x.InsertAbsences(It.IsAny<IEnumerable<AbsenceDTO>>()), Times.Never);
        }

        [Fact]
        public async Task SynchronizeAbsence_WhenGetAbsenceReturnsEmpty_LogsWarningAndReturns()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _JobExecutionRepoMock
                .Setup(x => x.GetLatestSuccessfulJobExecutionByName(nameof(EmployeeService.SynchronizeAbsence)))
                .ReturnsAsync(now);

            _severaClientMock
                .Setup(x => x.GetAbsence(It.IsAny<DateTime?>(), null))
                .ReturnsAsync(new List<AbsenceDTO>());

            // Act
            await _sut.SynchronizeAbsence();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GetAbsence returned empty")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _employeeRepoMock.Verify(x => x.GetEmployees(It.IsAny<bool>()), Times.Never);
            _employeeRepoMock.Verify(x => x.GetAbsenceByExternalIDs(It.IsAny<IEnumerable<Guid>>()), Times.Never);
            _employeeRepoMock.Verify(x => x.UpdateAbsences(It.IsAny<IEnumerable<AbsenceDTO>>()), Times.Never);
            _employeeRepoMock.Verify(x => x.InsertAbsences(It.IsAny<IEnumerable<AbsenceDTO>>()), Times.Never);
        }

        [Fact]
        public async Task SynchronizeAbsence_NoMatchingEmployees_LogsWarningAndDoesNotCallUpdateOrInsert()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var abs1 = new AbsenceDTO
            {
                ExternalId = Guid.NewGuid(),
                SeveraEmployeeId = Guid.NewGuid(),
                FromDate = DateTime.Today,
                ToDate = DateTime.Today.AddDays(1),
                Type = "Vacation"
            };
            var absences = new List<AbsenceDTO> { abs1 };

            _JobExecutionRepoMock
                .Setup(x => x.GetLatestSuccessfulJobExecutionByName(nameof(EmployeeService.SynchronizeAbsence)))
                .ReturnsAsync(now);

            _severaClientMock
                .Setup(x => x.GetAbsence(It.IsAny<DateTime?>(), null))
                .ReturnsAsync(absences);

            _employeeRepoMock
                .Setup(x => x.GetEmployees(It.IsAny<bool>()))
                .ReturnsAsync(new List<EmployeeDTO>());

            _employeeRepoMock
                .Setup(x => x.GetAbsenceByExternalIDs(It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync(new List<AbsenceDTO>());

            // Act
            await _sut.SynchronizeAbsence();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No employeeId found for the severa user")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _employeeRepoMock.Verify(x => x.UpdateAbsences(It.Is<IEnumerable<AbsenceDTO>>(u => !u.Any())), Times.Once);
            _employeeRepoMock.Verify(x => x.InsertAbsences(It.Is<IEnumerable<AbsenceDTO>>(i => !i.Any())), Times.Once);
        }

        [Fact]
        public async Task SynchronizeAbsence_WithMatchingEmployees_CallsUpdateAndInsertCorrectly()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var severaEmpId = Guid.NewGuid();
            var dbEmployee = new EmployeeDTO { Id = Guid.NewGuid(), SeveraId = severaEmpId };
            var absExistExternalId = Guid.NewGuid();
            var absNewExternalId = Guid.NewGuid();

            var absExisting = new AbsenceDTO
            {
                ExternalId = absExistExternalId,
                SeveraEmployeeId = severaEmpId,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today.AddDays(2),
                Type = "Sick"
            };

            var absNew = new AbsenceDTO
            {
                ExternalId = absNewExternalId,
                SeveraEmployeeId = severaEmpId,
                FromDate = DateTime.Today.AddDays(3),
                ToDate = DateTime.Today.AddDays(4),
                Type = "Vacation"
            };

            var absences = new List<AbsenceDTO> { absExisting, absNew };

            _JobExecutionRepoMock
                .Setup(x => x.GetLatestSuccessfulJobExecutionByName(nameof(EmployeeService.SynchronizeAbsence)))
                .ReturnsAsync(now);

            _severaClientMock
                .Setup(x => x.GetAbsence(It.IsAny<DateTime?>(), null))
                .ReturnsAsync(absences);

            _employeeRepoMock
                .Setup(x => x.GetEmployees(It.IsAny<bool>()))
                .ReturnsAsync(new List<EmployeeDTO> { dbEmployee });

            var dbAbsenceDto = new AbsenceDTO
            {
                ExternalId = absExistExternalId,
                SeveraEmployeeId = severaEmpId,
                EmployeeId = dbEmployee.Id,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today.AddDays(2),
                Type = "Sick"
            };
            _employeeRepoMock
                .Setup(x => x.GetAbsenceByExternalIDs(
                    It.Is<IEnumerable<Guid>>(ids => ids.Contains(absExistExternalId) && ids.Contains(absNewExternalId))))
                .ReturnsAsync(new List<AbsenceDTO> { dbAbsenceDto });

            List<AbsenceDTO> updatedList = null;
            List<AbsenceDTO> insertedList = null;

            _employeeRepoMock
                .Setup(x => x.UpdateAbsences(It.IsAny<IEnumerable<AbsenceDTO>>()))
                .Callback<IEnumerable<AbsenceDTO>>(list => updatedList = list.ToList())
                .Returns(Task.CompletedTask);

            _employeeRepoMock
                .Setup(x => x.InsertAbsences(It.IsAny<IEnumerable<AbsenceDTO>>()))
                .Callback<IEnumerable<AbsenceDTO>>(list => insertedList = list.ToList())
                .Returns(Task.CompletedTask);

            // Act
            await _sut.SynchronizeAbsence();

            // Assert
            Assert.All(absences, a => Assert.Equal(dbEmployee.Id, a.EmployeeId));

            Assert.Single(updatedList);
            Assert.Equal(absExistExternalId, updatedList[0].ExternalId);

            Assert.Single(insertedList);
            Assert.Equal(absNewExternalId, insertedList[0].ExternalId);
        }



    }
}
