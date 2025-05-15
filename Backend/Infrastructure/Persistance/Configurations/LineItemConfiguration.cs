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
    public class LineItemConfiguration : BaseEntityConfiguration<LineItem>
    {
        public void Configure(EntityTypeBuilder<LineItem> builder)
        {
            base.Configure(builder);

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id).ValueGeneratedNever();
            builder.Property(e => e.Description)
                .HasMaxLength(1000);
            builder.Property(e => e.ExternalId)
                .HasMaxLength(255);
            builder.Property(e => e.HourlyPrice).HasColumnType("decimal(5, 2)");
            builder.Property(e => e.Name)
                .HasMaxLength(255);

            builder.HasOne(d => d.Deal).WithMany(p => p.LineItems)
                .HasForeignKey(d => d.DealId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Responsible).WithMany(p => p.LineItems)
                .HasForeignKey(d => d.ResponsibleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
