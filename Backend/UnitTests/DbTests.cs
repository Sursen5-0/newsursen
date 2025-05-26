using Bogus;
using Castle.Core.Logging;
using Infrastructure.Persistance;
using Infrastructure.Persistance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public class DbTests
    {
        private SursenContext _context;
        public DbTests()
        {
        }

        [Fact]
        public async Task Ensure_EmployeesTable_Work()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<SursenContext>()
                .UseInMemoryDatabase("Sursen")
                .Options;
            _context = new SursenContext(options);
            var employee = new Employee
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
                Id = Guid.NewGuid(),
                ManagerId = Guid.NewGuid(),
                UpdatedAt = DateTime.UtcNow,
                WorkPhoneNumber = "",
                SeveraId = Guid.NewGuid(),
                PersonalPhoneNumber = "",
                Email = ""
            };
            // Act 
            _context.Employees.Add(employee);
            _context.SaveChanges();

            // Assert
            var test = _context.Employees.FirstOrDefault();
            Assert.NotNull(test);

        }


    }
}
