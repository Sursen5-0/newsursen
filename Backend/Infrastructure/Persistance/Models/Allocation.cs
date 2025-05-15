using System;
using System.Collections.Generic;

namespace Infrastructure.Persistance.Models;

public partial class Allocation : BaseEntity
{
    public Guid Id { get; set; }

    public string? Comment { get; set; }

    public Guid ProjectId { get; set; }

    public Guid EmployeeId { get; set; }

    public Guid LineItemId { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual LineItem LineItem { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;
}
