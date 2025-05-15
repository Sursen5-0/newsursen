using System;
using System.Collections.Generic;

namespace Infrastructure.Persistance.Models;

public partial class Skill : BaseEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

}
