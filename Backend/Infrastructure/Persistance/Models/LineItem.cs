using System;
using System.Collections.Generic;

namespace Infrastructure.Persistance.Models;

public partial class LineItem : BaseEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal HourlyPrice { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string ExternalId { get; set; } = null!;

    public Guid DealId { get; set; }

    public Guid ResponsibleId { get; set; }

    public virtual ICollection<Allocation> Allocations { get; set; } = new List<Allocation>();

    public virtual Deal Deal { get; set; } = null!;

    public virtual ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();

    public virtual Employee Responsible { get; set; } = null!;
}
