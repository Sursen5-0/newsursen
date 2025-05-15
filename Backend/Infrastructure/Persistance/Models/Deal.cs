using System;
using System.Collections.Generic;

namespace Infrastructure.Persistance.Models;

public partial class Deal : BaseEntity
{
    public Guid Id { get; set; }

    public string? ExternalId { get; set; }

    public bool IsFinalized { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Probability { get; set; }

    public DateOnly? ClosingDate { get; set; }

    public string CustomerName { get; set; } = null!;

    public Guid ResponsibleId { get; set; }

    public virtual ICollection<LineItem> LineItems { get; set; } = new List<LineItem>();

    public virtual Employee Responsible { get; set; } = null!;
}
