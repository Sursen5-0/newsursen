using Domain.Models;
using Infrastructure.Persistance.Models;

namespace Infrastructure.Persistance.Mappers
{
    public static class EmployeeSkillMapper
    {
        public static EmployeeSkillDTO ToDto(this EmployeeSkill employeeSkill)
        {
            return new EmployeeSkillDTO
            {
                Id = employeeSkill.Id,
                EmployeeId = employeeSkill.EmployeeId,
                YearsOfExperience = employeeSkill.YearsExperience,
                Name = employeeSkill.Name,
                ExternalId = employeeSkill.ExternalId,
                SkillId = employeeSkill.SkillId
            };
        }
        public static EmployeeSkill ToEntity(this EmployeeSkillDTO employeeSkillDto)
        {
            return new EmployeeSkill
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeSkillDto.EmployeeId,
                SkillId = employeeSkillDto.SkillId,
                YearsExperience = employeeSkillDto.YearsOfExperience,
                ExternalId = employeeSkillDto.ExternalId,
                Name = employeeSkillDto.Name,
            };
        }
    }
}
