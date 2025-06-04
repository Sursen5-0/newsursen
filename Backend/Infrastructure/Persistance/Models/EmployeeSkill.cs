using System;
using System.Collections.Generic;

namespace Infrastructure.Persistance.Models;

public partial class EmployeeSkill : BaseEntity
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }

    public Guid? SkillId { get; set; }

    public byte YearsExperience { get; set; }

    public string Name { get; set; } = null!;
    public string ExternalId { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;

    public virtual Skill? Skill { get; set; } = null!;
}
