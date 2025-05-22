using System;
using System.Collections.Generic;

namespace Infrastructure.Persistance.Models;

public partial class EmployeeContract : BaseEntity
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public DateOnly? ToDate { get; set; }

    public DateOnly FromDate { get; set; }

    public decimal ExpectedHours { get; set; }

    public Guid SeveraId { get; set; }

    public Guid EmployeeId { get; set; }


    public virtual Employee Employee { get; set; } = null!;
}
