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
        private readonly Mock<IFlowCaseClient> _flowcaseClientMock = new Mock<IFlowCaseClient>();
        private readonly Mock<ISkillRepository> _skillRepositoryMock = new Mock<ISkillRepository>();
        private EmployeeService _sut;

        public EmployeeServiceTests()
        {
            _sut = new EmployeeService(_severaClientMock.Object, _entraClientMock.Object, _flowcaseClientMock.Object, _employeeRepoMock.Object, _skillRepositoryMock.Object, _projectRepoMock.Object, _loggerMock.Object);

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
            _severaClientMock.Setup(x => x.GetProjects(It.IsAny<int?>())).ReturnsAsync(severaProjects);
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
            _severaClientMock.Setup(x => x.GetProjects(It.IsAny<int?>())).ReturnsAsync(severaProjects);
            _employeeRepoMock.Setup(x => x.GetEmployees(It.IsAny<bool>())).ReturnsAsync(new List<EmployeeDTO>());

            // Act
            await _sut.SynchronizeProjects();

            // Assert
            _projectRepoMock.Verify(x => x.InsertProjects(It.Is<List<ProjectDTO>>(x => x.Count == 1)), Times.Once);
            _loggerMock.VerifyLog(LogLevel.Error, Times.Once());
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
            _severaClientMock.Setup(x => x.GetProjects(It.IsAny<int?>())).ReturnsAsync(severaProjects);
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
            _severaClientMock.Setup(x => x.GetProjects(It.IsAny<int?>())).ReturnsAsync(severaProjects);
            _employeeRepoMock.Setup(x => x.GetEmployees(It.IsAny<bool>())).ReturnsAsync(employees);

            // Act
            await _sut.SynchronizeProjects();

            // Assert
            _projectRepoMock.Verify(x => x.InsertProjects(It.Is<List<ProjectDTO>>(x => x.Count == 0)), Times.Once);
            _projectRepoMock.Verify(x => x.UpdateProjects(It.Is<List<ProjectDTO>>(x => x.Count == 1)), Times.Once);
        }

    }
}
