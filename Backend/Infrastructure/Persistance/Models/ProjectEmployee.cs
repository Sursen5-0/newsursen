using System;
using System.Collections.Generic;

namespace Infrastructure.Persistance.Models;

public partial class ProjectEmployee : BaseEntity
{
    public Guid EmployeeId { get; set; }

    public Guid ProjectId { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;
}
