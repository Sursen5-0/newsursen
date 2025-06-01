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
using UnitTests.Common;

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
            _logger.VerifyLog(LogLevel.Warning, Times.Once());
        }


        [Fact]
        public async Task UpdateEmployeesAsync_NoErrorLog_WhenEntityNotFound()
        {
            // Arrange
            var dto = new EmployeeDTO { EntraId = Guid.NewGuid(), FirstName = "New" };
            var dtos = new List<EmployeeDTO> { dto };

            // Act
            await sut.UpdateEmployeesAsync(dtos);

            // Assert:
            _logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateEmployeesAsync_UpdatesFields_PreservesCreatedAt_UpdatesUpdatedAt()
        {
            // Arrange
            var entraId = Guid.NewGuid();
            var originalCreated = DateTime.UtcNow.AddDays(-2);
            var entity = new Employee
            {
                Id = Guid.NewGuid(),
                EntraId = entraId,
                FirstName = "OldFirst",
                LastName = "OldLast",
                Email = "old@domain.com",
                CreatedAt = originalCreated,
                UpdatedAt = originalCreated
            };
            _context.Employees.Add(entity);
            await _context.SaveChangesAsync();

            var dto = new EmployeeDTO
            {
                EntraId = entraId,
                FirstName = "NewFirst",
                LastName = "NewLast",
                Email = "new@domain.com",
                HireDate = DateOnly.FromDateTime(DateTime.UtcNow),
                LeaveDate = DateOnly.FromDateTime(DateTime.UtcNow)
            };
            var dtos = new List<EmployeeDTO> { dto };

            // Act
            await sut.UpdateEmployeesAsync(dtos);

            // Assert
            var updated = await _context.Employees.FirstAsync(e => e.EntraId == entraId);
            Assert.Equal("NewFirst", updated.FirstName);
            Assert.Equal("new@domain.com", updated.Email);
            Assert.Equal(originalCreated, updated.CreatedAt);
            Assert.True(updated.UpdatedAt > originalCreated, "UpdatedAt should be refreshed");
        }

        [Fact]
        public async Task InsertEmployeesAsync_AddsEntities()
        {
            // Arrange
            var existing = _context.Employees.ToList();
            if (existing.Any())
            {
                _context.Employees.RemoveRange(existing);
                await _context.SaveChangesAsync();
            }

            var dto1 = new EmployeeDTO
            {
                EntraId = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john@doe.com"
            };
            var dto2 = new EmployeeDTO
            {
                EntraId = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@smith.com"
            };
            var dtos = new List<EmployeeDTO> { dto1, dto2 };

            // Act
            await sut.InsertEmployeesAsync(dtos);

            // Assert
            var all = await _context.Employees.ToListAsync();
            Assert.Contains(all, e => e.EntraId == dto1.EntraId && e.FirstName == "John");
            Assert.Contains(all, e => e.EntraId == dto2.EntraId && e.FirstName == "Jane");
        }
    }
}

