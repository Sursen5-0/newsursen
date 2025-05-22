using Infrastructure.Persistance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Configurations
{
    public class EmployeeContractConfiguration : BaseEntityConfiguration<EmployeeContract>
    {
        public override void Configure(EntityTypeBuilder<EmployeeContract> builder)
        {
            BaseConfigure(builder);

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id).ValueGeneratedNever();
            builder.Property(e => e.ExpectedHours).HasColumnType("decimal(5, 2)");
            builder.Property(e => e.Title)
                .HasMaxLength(255);

            builder.HasOne(d => d.Employee).WithMany(p => p.EmployeeContracts)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
