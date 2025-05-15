using Infrastructure.Persistance.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
namespace Infrastructure.Persistance.Configurations
{
    public class AllocationConfiguration : BaseEntityConfiguration<Allocation>
    {
        public void Configure(EntityTypeBuilder<Allocation> builder)
        {
            base.Configure(builder);

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id).ValueGeneratedNever();
            builder.Property(e => e.Comment).HasMaxLength(1000);

            builder.HasOne(d => d.Employee).WithMany(p => p.Allocations)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.LineItem).WithMany(p => p.Allocations)
                    .HasForeignKey(d => d.LineItemId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Project).WithMany(p => p.Allocations)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}