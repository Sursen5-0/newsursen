using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.FlowCase.Models;
using Infrastructure.Persistance.Models;

namespace Infrastructure.FlowCase
{
    public static class SkillMapper
    {
        public static SkillDTO ToDto (this FlowcaseSkillModel skill)
        {
            return new SkillDTO()
            {
                Id = Guid.NewGuid(),
                SkillId = skill.SkillId,
                SkillName = skill.Tags.Dk,
                SkillProficiency = skill.Proficiency,
                SkillTotalDurationInYears = skill.TotalDurationInYears
                };
        }


    }
}
