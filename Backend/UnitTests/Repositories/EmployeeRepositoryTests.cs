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
            var options = new DbContextOptionsBuilder<SursenContext>().UseSqlite("DataSource=:memory:");
            _logger = new Mock<ILogger<EmployeeRepository>>();
            _context = new SursenContext(options.Options);
            _context.Database.OpenConnection();
            _context.Database.Migrate();
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

            _context.Employees.Add(new Employee { Id = validEmployeeId });
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
        private List<EmployeeContractDTO> GenerateContracts()
        {
            var now = DateTime.UtcNow;

            return new List<EmployeeContractDTO>
            {
                new ()
                {
                    Id = Guid.NewGuid(),
                    Title = "Contract 1",
                    FromDate = now.AddMonths(-5),
                    ToDate = now.AddMonths(1),
                    ExpectedHours = 160,
                    SeveraId = Guid.NewGuid(),
                    EmployeeId = Guid.NewGuid()
                },
                new ()
                {
                    Id = Guid.NewGuid(),
                    Title = "Contract 2",
                    FromDate = now.AddMonths(-4),
                    ToDate = null,
                    ExpectedHours = 165,
                    SeveraId = Guid.NewGuid(),
                    EmployeeId = Guid.NewGuid()
                },
                new ()
                {
                    Id = Guid.NewGuid(),
                    Title = "Contract 3",
                    FromDate = now.AddMonths(-3),
                    ToDate = now.AddMonths(2),
                    ExpectedHours = 170,
                    SeveraId = Guid.NewGuid(),
                    EmployeeId = Guid.NewGuid()
                },
                new ()
                {
                    Id = Guid.NewGuid(),
                    Title = "Contract 4",
                    FromDate = now.AddMonths(-2),
                    ToDate = null,
                    ExpectedHours = 175,
                    SeveraId = Guid.NewGuid(),
                    EmployeeId = Guid.NewGuid()
                },
                new ()
                {
                    Id = Guid.NewGuid(),
                    Title = "Contract 5",
                    FromDate = now.AddMonths(-1),
                    ToDate = now.AddMonths(3),
                    ExpectedHours = 180,
                    SeveraId = Guid.NewGuid(),
                    EmployeeId = Guid.NewGuid()
                }
            };
        }
    }
}

