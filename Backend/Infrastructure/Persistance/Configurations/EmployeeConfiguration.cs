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
    public class EmployeeConfiguration : BaseEntityConfiguration<Employee>
    {
        public override void Configure(EntityTypeBuilder<Employee> builder)
        {
            BaseConfigure(builder);

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id).ValueGeneratedNever();
            builder.Property(e => e.WorkPhoneNumber).HasMaxLength(20);
            builder.Property(e => e.PersonalPhoneNumber).HasMaxLength(20);
            builder.Property(e => e.FirstName).HasMaxLength(255);
            builder.Property(e => e.Email).HasMaxLength(255);
            builder.Property(e => e.LastName).HasMaxLength(255);
            builder.Property(e => e.FlowCaseId).HasMaxLength(24).IsFixedLength().IsUnicode(false);
            builder.Property(e => e.FlowCaseCVId).HasMaxLength(24).IsFixedLength().IsUnicode(false);
            builder.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            builder.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            builder.HasOne(d => d.Manager)
                .WithMany(p => p.Employees)
                .HasForeignKey(d => d.ManagerId);


        }
    }
}
