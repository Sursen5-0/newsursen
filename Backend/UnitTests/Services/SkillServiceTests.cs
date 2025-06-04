using Application.Services;
using Domain.Interfaces.ExternalClients;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Services
{
    public class SkillServiceTests
    {
      
        private readonly Mock<ILogger<SkillService>> _loggerMock = new Mock<ILogger<SkillService>>();
        private readonly Mock<IFlowCaseClient> _flowcaseClientMock = new Mock<IFlowCaseClient>();
        private readonly Mock<ISkillRepository> _skillRepositoryMock = new Mock<ISkillRepository>();
        private SkillService _sut;

        public SkillServiceTests()
        {
            _sut = new SkillService(_flowcaseClientMock.Object, _skillRepositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task SynchronizeSkillsFromFlowcaseAsync_Should_Add_New_Skills_When_They_Do_Not_Exist()
        {
            // Arrange
            var skills = new List<SkillDTO>
            {
                new SkillDTO { ExternalId = "1", SkillName = "Skill1" },
                new SkillDTO { ExternalId = "2", SkillName = "Skill2" }
            };
            _flowcaseClientMock.Setup(x => x.GetSkillsFromFlowcaseAsync()).ReturnsAsync(skills);
            _skillRepositoryMock.Setup(x => x.GetAllSkillsAsync()).ReturnsAsync(new List<SkillDTO>());
            // Act
            await _sut.SynchronizeSkillsFromFlowcaseAsync();
            // Assert
            _skillRepositoryMock.Verify(x => x.AddSkillAsync(skills), Times.Once);
        }

        [Fact]
        public async Task SynchronizeSkillsFromFlowcaseAsync_Should_Skip_Existing_Skills()
        {
            // Arrange
            var existingSkills = new List<SkillDTO>
            {
                new SkillDTO { ExternalId = "1", SkillName = "Skill1" },
                new SkillDTO { ExternalId = "2", SkillName = "Skill2" }
            };
            var newSkills = new List<SkillDTO>
            {
                new SkillDTO { ExternalId = "1", SkillName = "Skill1" }
            };
            _flowcaseClientMock.Setup(x => x.GetSkillsFromFlowcaseAsync()).ReturnsAsync(newSkills);
            _skillRepositoryMock.Setup(x => x.GetAllSkillsAsync()).ReturnsAsync(existingSkills);
            // Act
            await _sut.SynchronizeSkillsFromFlowcaseAsync();
            // Assert
            _skillRepositoryMock.Verify(x => x.AddSkillAsync(It.IsAny<IEnumerable<SkillDTO>>()), Times.Never);
        }

        [Fact]
        public async Task SynchronizeSkillsFromFlowcaseAsync_Should_Log_Warning_When_No_Skills_Are_Found()
        {
            // Arrange
            _flowcaseClientMock.Setup(x => x.GetSkillsFromFlowcaseAsync()).ReturnsAsync(new List<SkillDTO>());
            // Act
            await _sut.SynchronizeSkillsFromFlowcaseAsync();
            // Assert
            _loggerMock.Verify(x => x.LogWarning("No skills found in Flowcase, skipping synchronization."), Times.Once);
        }
    }
}
