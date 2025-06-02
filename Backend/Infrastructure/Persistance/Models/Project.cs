using System;
using System.Collections.Generic;

namespace Infrastructure.Persistance.Models;

public partial class Project : BaseEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public Guid ExternalId { get; set; }
    public bool IsClosed { get; set; }
    public Guid? ResponsibleId { get; set; }
    public Guid ExternalResponsibleId { get; set; }

    public virtual ICollection<Allocation> Allocations { get; set; } = new List<Allocation>();
    public virtual ICollection<ProjectPhase> Phases { get; set; } = new List<ProjectPhase>();
    public virtual Employee? Responsible { get; set; }
}
