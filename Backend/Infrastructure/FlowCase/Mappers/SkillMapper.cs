using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.FlowCase.Models;
using Infrastructure.Persistance.Models;

namespace Infrastructure.FlowCase.Mappers
{
    public static class SkillMapper
    {
        public static SkillDTO ToSkillDto(this FlowcaseSkillModel skill)
        {
            return new SkillDTO()
            {
                ExternalId = skill.SkillId,
                SkillName = skill.Values.Name,
            };
        }
        public static EmployeeSkillDTO ToEmployeeSkillDto(this FlowcaseSkillModel skill)
        {
            return new EmployeeSkillDTO()
            {
                YearsOfExperience = skill.TotalDurationInYears,
                Name = skill.Tags.Name,
                ExternalId = skill.SkillId,
            };
        }



    }
}
