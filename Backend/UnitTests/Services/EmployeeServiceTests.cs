using Application.Services;
using Bogus;
using Domain.Interfaces.ExternalClients;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Infrastructure.Persistance.Repositories;
using Infrastructure.Severa;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace UnitTests.Services
{
    public class EmployeeServiceTests
    {
        private readonly Mock<IEmployeeRepository> _employeeRepositoryMock = new Mock<IEmployeeRepository>();
        private readonly Mock<ISeveraClient> _severaClientMock = new Mock<ISeveraClient>();
        private readonly Mock<ILogger<EmployeeService>> _loggerMock = new Mock<ILogger<EmployeeService>>();
        private readonly Mock<IEntraClient> _entraClientMock = new Mock<IEntraClient>();
        private EmployeeService _service;

        [Fact]
        public async Task SynchronizeContracts_WithNoEmployees_DoesntCallGetWorkContractByUserId()
        {
            // Arrange
            var emptyEmployeeList = new List<EmployeeDTO>();
            _employeeRepositoryMock.Setup(x => x.GetEmployees(It.IsAny<bool>())).ReturnsAsync(emptyEmployeeList);
            _employeeRepositoryMock.Setup(x => x.InsertEmployeeContracts(It.IsAny<IEnumerable<EmployeeContractDTO>>())).Verifiable();
            
            _service = new EmployeeService(_severaClientMock.Object, _entraClientMock.Object, _employeeRepositoryMock.Object, _loggerMock.Object);


            // Act
            await _service.SynchronizeContracts();

            // Assert
            _severaClientMock.Verify(x=> x.GetWorkContractByUserId(It.IsAny<Guid>()), Times.Never);
        }


        [Fact]
        public async Task SynchronizeContracts_WithOneEmployeesWithoutSeveraID_DoesntCallGetWorkContractByUserId()
        {
            // Arrange
            var emptyEmployeeList = new List<EmployeeDTO>() { new EmployeeDTO() { Id = Guid.NewGuid()} };
            _employeeRepositoryMock.Setup(x => x.GetEmployees(It.IsAny<bool>())).ReturnsAsync(emptyEmployeeList);
            _employeeRepositoryMock.Setup(x => x.InsertEmployeeContracts(It.IsAny<IEnumerable<EmployeeContractDTO>>())).Verifiable();

            _service = new EmployeeService(_severaClientMock.Object, _entraClientMock.Object, _employeeRepositoryMock.Object, _loggerMock.Object);


            // Act
            await _service.SynchronizeContracts();

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
            
            _employeeRepositoryMock.Setup(x => x.GetEmployees(It.IsAny<bool>())).ReturnsAsync(faker.Generate(times));
            _employeeRepositoryMock.Setup(x => x.InsertEmployeeContracts(It.IsAny<IEnumerable<EmployeeContractDTO>>())).Verifiable();
            _severaClientMock.Setup(x => x.GetWorkContractByUserId(It.IsAny<Guid>())).ReturnsAsync(new EmployeeContractDTO());

            _service = new EmployeeService(_severaClientMock.Object, _entraClientMock.Object, _employeeRepositoryMock.Object, _loggerMock.Object);


            // Act
            await _service.SynchronizeContracts();

            // Assert
            _severaClientMock.Verify(x => x.GetWorkContractByUserId(It.IsAny<Guid>()),Times.Exactly(times) );
        }

        [Fact]
        public async Task SynchronizeUnmappedSeveraIds_WithOneEmployeesWithoutSeveraID_DoesntCallGetUserByEmail()
        {
            // Arrange
            var faker = new Faker<EmployeeDTO>()
                .RuleFor(x => x.Id, x => Guid.NewGuid())
                .RuleFor(x => x.SeveraId, x => null);

            _employeeRepositoryMock.Setup(x => x.GetEmployeeWithoutSeveraIds(It.IsAny<bool>())).ReturnsAsync(faker.Generate(1));
            _severaClientMock.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(new SeveraEmployeeModel());
            _employeeRepositoryMock.Setup(x => x.UpdateSeveraIds(It.IsAny<List<SeveraEmployeeModel>>())).Verifiable();

            _service = new EmployeeService(_severaClientMock.Object, _entraClientMock.Object, _employeeRepositoryMock.Object, _loggerMock.Object);


            // Act
            await _service.SynchronizeContracts();

            // Assert
            _severaClientMock.Verify(x => x.GetUserByEmail(It.IsAny<string>()), Times.Never);
        }

    }
}
