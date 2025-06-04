using System;
using Infrastructure.Persistance.Mappers;
using Infrastructure.Persistance.Models;
using Domain.Models;
using Xunit;

namespace UnitTests.Mappers
{
    public class ProjectPhaseMapperTests
    {
        [Fact]
        public void ToDto_MapsAllPropertiesCorrectly()
        {
            // Arrange
            var projectPhase = new ProjectPhase
            {
                Id = Guid.NewGuid(),
                Name = "Phase 1",
                DeadLine = new DateTime(2025, 12, 31),
                ExternalId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                ParentPhaseId = Guid.NewGuid(),
                StartDate = new DateTime(2025, 1, 1)
            };

            // Act
            var dto = projectPhase.ToDto();

            // Assert
            Assert.Equal(projectPhase.Id, dto.Id);
            Assert.Equal(projectPhase.Name, dto.Name);
            Assert.Equal(projectPhase.DeadLine, dto.DeadLine);
            Assert.Equal(projectPhase.ExternalId, dto.ExternalId);
            Assert.Equal(projectPhase.ProjectId, dto.ProjectId);
            Assert.Equal(projectPhase.ParentPhaseId, dto.ParentPhaseId);
            Assert.Equal(projectPhase.StartDate, dto.StartDate);
        }

        [Fact]
        public void ToEntity_SetsNewIdAndMapsOtherPropertiesCorrectly()
        {
            // Arrange
            var dto = new ProjectPhaseDTO
            {
                Id = Guid.NewGuid(), // DTO Id is not used in entity
                Name = "Phase 2",
                DeadLine = new DateTime(2026, 6, 30),
                ExternalId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                ParentPhaseId = Guid.NewGuid(),
                StartDate = new DateTime(2026, 1, 1)
            };

            // Act
            var entity = dto.ToEntity();

            // Assert
            Assert.NotEqual(Guid.Empty, entity.Id);
            Assert.Equal(dto.Name, entity.Name);
            Assert.Equal(dto.DeadLine, entity.DeadLine);
            Assert.Equal(dto.ExternalId, entity.ExternalId);
            Assert.Equal(dto.ProjectId, entity.ProjectId);
            Assert.Equal(dto.ParentPhaseId, entity.ParentPhaseId);
            Assert.Equal(dto.StartDate, entity.StartDate);
        }
    }
}
