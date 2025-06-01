using Domain.Models;
using Infrastructure.Persistance.Models;

namespace Infrastructure.Persistance.Mappers
{
    public static class EmployeeSkillMapper
    {
        public static EmployeeSkillDTO ToDTO(EmployeeSkill employeeSkill)
        {
            return new EmployeeSkillDTO
            {
                Id = employeeSkill.EmployeeId,
                SkillId = employeeSkill.SkillId,
                YearsOfExperience = employeeSkill.YearsExperience
            };
        }
        public static EmployeeSkill ToEntity(EmployeeSkillDTO employeeSkillDto)
        {
            return new EmployeeSkill
            {
                EmployeeId = employeeSkillDto.Id,
                SkillId = employeeSkillDto.SkillId,
                YearsExperience = employeeSkillDto.YearsOfExperience
            };
        }
    }
}
