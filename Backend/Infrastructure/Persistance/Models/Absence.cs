using System;
using System.Collections.Generic;

namespace Infrastructure.Persistance.Models;

public partial class Absence : BaseEntity
{
    public Guid Id { get; set; }
    public string? Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid ExternalId { get; set; }
    public virtual Employee Employee { get; set; } = null!;
}