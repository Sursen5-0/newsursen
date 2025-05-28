using Domain.Models;
using Infrastructure.Persistance.Models;
using Infrastructure.Persistance.Repositories;
using Infrastructure.Persistance;
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
    public class ProjectRepositoryTests
    {
        private ProjectRepository sut;
        private readonly SursenContext _context;
        private readonly Mock<ILogger<ProjectRepository>> _logger;


        public ProjectRepositoryTests()
        {
            _logger = new Mock<ILogger<ProjectRepository>>();
            var options = new DbContextOptionsBuilder<SursenContext>()
                .UseInMemoryDatabase("Sursen")
                .Options;
            _context = new SursenContext(options);
            sut = new ProjectRepository(_context, _logger.Object);
        }

        [Fact]
        public async Task InsertEmployeeContracts_ThrowsArgumentException_GivenNullParamter()
        {
            // Arrange
            IEnumerable<ProjectDTO> listToInsert = null!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.InsertProjects(listToInsert));

        }

    }
}
