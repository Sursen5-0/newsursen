using Domain.Models;
using Infrastructure.Persistance;
using Infrastructure.Persistance.Mappers;
using Infrastructure.Persistance.Models;
using Infrastructure.Persistance.Repositories;
using Infrastructure.Severa;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Repositories
{
    public class EmployeeRepositoryTests
    {
        private EmployeeRepository sut;
        private readonly SursenContext _context;
        private readonly Mock<ILogger<EmployeeRepository>> _logger;


        public EmployeeRepositoryTests()
        {
            _logger = new Mock<ILogger<EmployeeRepository>>();
            var options = new DbContextOptionsBuilder<SursenContext>()
                .UseInMemoryDatabase("Sursen")
                .Options;
            _context = new SursenContext(options);
            sut = new EmployeeRepository(_context, _logger.Object);
        }

        [Fact]
        public async Task InsertEmployeeContracts_ThrowsArgumentException_GivenNullParamter()
        {
            // Arrange
            IEnumerable<EmployeeContractDTO> listToInsert = null!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.InsertEmployeeContracts(listToInsert));

        }

        [Fact]
        public async Task InsertEmployeeContracts_ThrowsArgumentExcdeption_GivenNullParamter()
        {
            // Arrange
            var validEmployeeId = Guid.NewGuid();
            var unknownEmployeeId = Guid.NewGuid();

            _context.Employees.Add(new Employee
            {
                Birthdate = DateOnly.MinValue,
                BusinessUnitId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                EntraId = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "Test",
                FlowCaseId = "test",
                HireDate = DateOnly.MinValue,
                HubSpotId = "test",
                LeaveDate = DateOnly.MinValue,
                Id = validEmployeeId,
                ManagerId = Guid.NewGuid(),
                UpdatedAt = DateTime.UtcNow,
                WorkPhoneNumber = "",
                SeveraId = Guid.NewGuid(),
                PersonalPhoneNumber = "",
                Email = ""
            });
            await _context.SaveChangesAsync();

            var contracts = new List<EmployeeContractDTO>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Valid Contract",
                    FromDate = DateTime.UtcNow.AddMonths(-1),
                    ToDate = DateTime.UtcNow.AddMonths(1),
                    ExpectedHours = 160,
                    SeveraId = Guid.NewGuid(),
                    EmployeeId = validEmployeeId
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Invalid Contract",
                    FromDate = DateTime.UtcNow.AddMonths(-1),
                    ToDate = DateTime.UtcNow.AddMonths(1),
                    ExpectedHours = 160,
                    SeveraId = Guid.NewGuid(),
                    EmployeeId = unknownEmployeeId
                }
            };

            // Act
            await sut.InsertEmployeeContracts(contracts);

            // Assert
            _logger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains($"EmployeeId {unknownEmployeeId}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

