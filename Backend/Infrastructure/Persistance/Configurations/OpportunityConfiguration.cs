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
    public class OpportunityConfiguration : BaseEntityConfiguration<Opportunity>
    {
        public override void Configure(EntityTypeBuilder<Opportunity> builder)
        {
            BaseConfigure(builder);

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id).ValueGeneratedNever();
            builder.Property(e => e.Comment)
                .HasMaxLength(1000);

            builder.HasOne(d => d.Employee).WithMany(p => p.Opportunities)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.LineItem).WithMany(p => p.Opportunities)
                .HasForeignKey(d => d.LineItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
