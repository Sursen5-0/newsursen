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
        public async Task UpdateEmployeeAsync_ThrowsInvalidOperationException_WhenEntityNotFound()
        {
            // Arrange
            var dto = new EmployeeDTO { EntraId = Guid.NewGuid() };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.UpdateEmployeeAsync(dto));

            // Verify that we logged an error via your VerifyLog helper
            _logger.VerifyLog(LogLevel.Error, Times.Once());
        }

        [Fact]
        public async Task UpdateEmployeeAsync_UpdatesFields_PreservesCreatedAt_UpdatesUpdatedAt()
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

            // Act
            await sut.UpdateEmployeeAsync(dto);

            // Assert
            var updated = await _context.Employees.FirstAsync(e => e.EntraId == entraId);
            Assert.Equal("NewFirst", updated.FirstName);
            Assert.Equal("new@domain.com", updated.Email);
            Assert.Equal(originalCreated, updated.CreatedAt);
            Assert.True(updated.UpdatedAt > originalCreated, "UpdatedAt should be refreshed");
        }

        [Fact]
        public async Task InsertEmployeeAsync_AddsEntityWithAuditFields()
        {
            // Arrange
            var dto = new EmployeeDTO
            {
                EntraId = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john@doe.com",
                HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
                LeaveDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                WorkPhoneNumber = "123",
                PersonalPhoneNumber = "456",
            };

            // Act
            await sut.InsertEmployeeAsync(dto);

            // Assert
            var entity = await _context.Employees.FirstOrDefaultAsync(e => e.EntraId == dto.EntraId);
            Assert.NotNull(entity);
            Assert.Equal(dto.FirstName, entity.FirstName);
            Assert.Equal(dto.LastName, entity.LastName);
            Assert.Equal(dto.Email, entity.Email);
            Assert.Equal(entity.CreatedAt, entity.UpdatedAt);
        }
    }
}

