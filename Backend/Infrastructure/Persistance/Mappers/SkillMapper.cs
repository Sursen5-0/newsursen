using Domain.Models;
using Infrastructure.Persistance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Mappers
{
    public static class SkillMapper
    {

        public static SkillDTO ToDTO(this Skill skill)
        {
            return new SkillDTO()
            {
                Id = skill.Id,
                SkillName = skill.Name,
                SkillId = skill.ExternalId
            };
        }

        public static Skill ToEntity(this SkillDTO skill)
        {
            return new Skill()
            {
                Id = skill.Id,
                Name = skill.SkillName,
                ExternalId = skill.SkillId
            };
        }
    }
}
