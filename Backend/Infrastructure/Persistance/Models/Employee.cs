using System;
using System.Collections.Generic;

namespace Infrastructure.Persistance.Models;

public partial class Employee : BaseEntity
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateOnly HireDate { get; set; }

    public DateOnly? LeaveDate { get; set; }

    public DateOnly Birthdate { get; set; }

    public Guid BusinessUnitId { get; set; }

    public string? WorkPhoneNumber { get; set; }

    public string? PersonalPhoneNumber { get; set; }

    public string HubSpotId { get; set; } = null!;
    public Guid SeveraId { get; set; }
    public Guid? ManagerId { get; set; }

    public Guid EntraId { get; set; }

    public string FlowCaseId { get; set; } = null!;

    public virtual ICollection<Absence> Absences { get; set; } = new List<Absence>();

    public virtual ICollection<Allocation> Allocations { get; set; } = new List<Allocation>();

    public virtual ICollection<Deal> Deals { get; set; } = new List<Deal>();

    public virtual ICollection<EmployeeContract> EmployeeContracts { get; set; } = new List<EmployeeContract>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<LineItem> LineItems { get; set; } = new List<LineItem>();

    public virtual Employee? Manager { get; set; }

    public virtual ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
}
