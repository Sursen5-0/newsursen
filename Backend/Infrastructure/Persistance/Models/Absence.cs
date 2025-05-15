using System;
using System.Collections.Generic;

namespace Infrastructure.Persistance.Models;

public partial class Absence : BaseEntity
{
    public Guid Id { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public Guid EmployeeId { get; set; }
    public virtual Employee Employee { get; set; } = null!;
}