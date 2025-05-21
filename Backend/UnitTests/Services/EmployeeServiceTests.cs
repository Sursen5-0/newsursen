using Application.Services;
using Domain.Interfaces.ExternalClients;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
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

        private readonly EmployeeService _service;

        public EmployeeServiceTests()
        {
            _service = new EmployeeService(_severaClientMock.Object, _employeeRepositoryMock.Object, _loggerMock.Object);
        }
    }
}
