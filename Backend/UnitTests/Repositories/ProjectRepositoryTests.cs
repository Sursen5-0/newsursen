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
using System.Threading.Tasks;
using Xunit;

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
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new SursenContext(options);
            sut = new ProjectRepository(_context, _logger.Object);
        }

        [Fact]
        public async Task InsertProjects_ThrowsArgumentNullException_GivenNull()
        {
            // Arrange
            IEnumerable<ProjectDTO> nullList = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.InsertProjects(nullList));
        }

        [Fact]
        public async Task InsertProjects_AddsNewProjectsToDatabase()
        {
            // Arrange
            var dtos = new List<ProjectDTO>
            {
                new ProjectDTO { Name = "Proj A", Description = "Desc A", ExternalId = Guid.NewGuid(), IsClosed = false, ExternalOwnerId = Guid.NewGuid() },
                new ProjectDTO { Name = "Proj B", Description = "Desc B", ExternalId = Guid.NewGuid(), IsClosed = true, ExternalOwnerId = Guid.NewGuid() }
            };

            // Act
            await sut.InsertProjects(dtos);
            var dbList = await _context.Projects.ToListAsync();

            // Assert
            Assert.Contains(dbList, x => x.ExternalId == dtos[0].ExternalId && !x.IsClosed);
            Assert.Contains(dbList, x => x.ExternalId == dtos[1].ExternalId && x.IsClosed);
        }

        [Fact]
        public async Task GetProjects_ReturnsInsertedProjects_WhenIncludeInactiveTrue()
        {
            // Arrange
            var project1 = new Project { Id = Guid.NewGuid(), Name = "Active", ExternalId = Guid.NewGuid(), IsClosed = false, ExternalResponsibleId = Guid.NewGuid() };
            var project2 = new Project { Id = Guid.NewGuid(), Name = "Closed", ExternalId = Guid.NewGuid(), IsClosed = true, ExternalResponsibleId = Guid.NewGuid() };
            _context.Projects.Add(project1);
            _context.Projects.Add(project2);
            await _context.SaveChangesAsync();

            // Act
            var result = await sut.GetProjects(includeInactive: true);

            // Assert
            Assert.Contains(result, x => x.ExternalId == project1.ExternalId);
            Assert.Contains(result, x => x.ExternalId == project2.ExternalId);
        }

        [Fact]
        public async Task GetProjects_ExcludesClosed_WhenIncludeInactiveFalse()
        {
            // Arrange
            var openProject = new Project { Id = Guid.NewGuid(), Name = "Open1", ExternalId = Guid.NewGuid(), IsClosed = false, ExternalResponsibleId = Guid.NewGuid() };
            var closedProject = new Project { Id = Guid.NewGuid(), Name = "Closed1", ExternalId = Guid.NewGuid(), IsClosed = true, ExternalResponsibleId = Guid.NewGuid() };
            _context.Projects.Add(openProject);
            _context.Projects.Add(closedProject);
            await _context.SaveChangesAsync();

            // Act
            var result = await sut.GetProjects(includeInactive: false);

            // Assert
            Assert.Contains(result, x => x.ExternalId == openProject.ExternalId);
            Assert.DoesNotContain(result, x => x.ExternalId == closedProject.ExternalId);
        }

        [Fact]
        public async Task UpdateProjects_UpdatesProjectNameAndStatus()
        {
            // Arrange
            var extId = Guid.NewGuid();
            var initial = new Project { Id = Guid.NewGuid(), Name = "Original", Description = "D1", ExternalId = extId, IsClosed = false, ExternalResponsibleId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow.AddDays(-1) };
            _context.Projects.Add(initial);
            await _context.SaveChangesAsync();

            var updatedDto = new ProjectDTO { Name = "Updated", Description = "D2", ExternalId = extId, IsClosed = true, ExternalOwnerId = initial.ExternalResponsibleId };

            // Act
            await sut.UpdateProjects(new List<ProjectDTO> { updatedDto });
            var dbEntity = await _context.Projects.SingleAsync(p => p.ExternalId == extId);

            // Assert
            Assert.Equal("Updated", dbEntity.Name);
            Assert.True(dbEntity.IsClosed);
        }

        [Fact]
        public async Task GetPhases_ReturnsInsertedPhases_WhenIncludeInactiveTrue()
        {
            // Arrange
            var now = DateTime.Now;
            var phase1 = new ProjectPhase { Id = Guid.NewGuid(), Name = "Phase1", ExternalId = Guid.NewGuid(), ProjectId = Guid.NewGuid(), DeadLine = now.AddDays(1) };
            var phase2 = new ProjectPhase { Id = Guid.NewGuid(), Name = "Phase2", ExternalId = Guid.NewGuid(), ProjectId = Guid.NewGuid(), DeadLine = now.AddDays(-1) };
            _context.ProjectPhases.Add(phase1);
            _context.ProjectPhases.Add(phase2);
            await _context.SaveChangesAsync();

            // Act
            var result = await sut.GetPhases(includeInactive: true);

            // Assert
            Assert.Contains(result, x => x.ExternalId == phase1.ExternalId);
            Assert.Contains(result, x => x.ExternalId == phase2.ExternalId);
        }


        [Fact]
        public async Task InsertPhases_AddsPhases_WithParentChildRelation()
        {
            // Arrange
            var parentId = Guid.NewGuid();
            var parentDto = new ProjectPhaseDTO { Name = "Parent", ExternalId = parentId, ProjectId = Guid.NewGuid() };
            var childDto = new ProjectPhaseDTO { Name = "Child", ExternalId = Guid.NewGuid(), ProjectId = parentDto.ProjectId, ExternalParentPhaseId = parentId };

            // Act
            await sut.InsertPhases(new[] { parentDto, childDto });
            var parentEntity = await _context.ProjectPhases.SingleAsync(p => p.ExternalId == parentId);
            var childEntity = await _context.ProjectPhases.SingleAsync(p => p.ExternalId == childDto.ExternalId);

            // Assert
            Assert.Equal(parentEntity.Id, childEntity.ParentPhaseId);
            Assert.Contains(childEntity, parentEntity.UnderPhases);
        }

        [Fact]
        public async Task UpdatePhases_UpdatesPhaseName()
        {
            // Arrange
            var phaseExtId = Guid.NewGuid();
            var entity = new ProjectPhase { Id = Guid.NewGuid(), Name = "OldName", ExternalId = phaseExtId, ProjectId = Guid.NewGuid(), DeadLine = DateTime.Now.AddDays(10), CreatedAt = DateTime.Now.AddDays(-2) };
            _context.ProjectPhases.Add(entity);
            await _context.SaveChangesAsync();

            var updatedDto = new ProjectPhaseDTO { ExternalId = phaseExtId, Name = "NewName", ProjectId = entity.ProjectId, DeadLine = entity.DeadLine };

            // Act
            await sut.UpdatePhases(new[] { updatedDto });
            var updatedEntity = await _context.ProjectPhases.SingleAsync(p => p.ExternalId == phaseExtId);

            // Assert
            Assert.Equal("NewName", updatedEntity.Name);
        }
    }
}
