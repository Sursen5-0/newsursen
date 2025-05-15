using System;
using System.Collections.Generic;

namespace Infrastructure.Persistance.Models;

public partial class Project : BaseEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public Guid? ExternalId { get; set; }


    public virtual ICollection<Allocation> Allocations { get; set; } = new List<Allocation>();
}
