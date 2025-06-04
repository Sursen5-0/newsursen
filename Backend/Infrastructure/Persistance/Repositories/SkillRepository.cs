using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistance.Mappers;

namespace Infrastructure.Persistance.Repositories
{
    public class SkillRepository(SursenContext _db, ILogger<SkillRepository> _logger) : ISkillRepository
    {
        public async Task AddSkillAsync(IEnumerable<SkillDTO> skills)
        {
            var skillEntity = skills.Select(skill => skill.ToEntity()).ToList();
            _db.Skills.AddRange(skillEntity);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<SkillDTO>> GetAllSkillsAsync()
        {
            _logger.LogInformation("Retrieving all skills from the database.");
            return await _db.Skills.Select(x => x.ToDTO()).ToListAsync();
        }
    }
}
