using System;
using System.Collections.Generic;
using Infrastructure.Persistance.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance;

public partial class SursenContext : DbContext
{
    public SursenContext()
    {
    }

    public SursenContext(DbContextOptions<SursenContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Absence> Absences { get; set; }

    public virtual DbSet<Allocation> Allocations { get; set; }

    public virtual DbSet<Deal> Deals { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeContract> EmployeeContracts { get; set; }

    public virtual DbSet<EmployeeSkill> EmployeeSkills { get; set; }

    public virtual DbSet<LineItem> LineItems { get; set; }

    public virtual DbSet<Opportunity> Opportunities { get; set; }

    public virtual DbSet<Project> Projects { get; set; }
    public virtual DbSet<ProjectPhase> ProjectPhases { get; set; }
    public virtual DbSet<JobExecution> JobExecutions { get; set; }

    public virtual DbSet<ProjectEmployee> ProjectEmployees { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    { 
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SursenContext).Assembly);
    }
}
