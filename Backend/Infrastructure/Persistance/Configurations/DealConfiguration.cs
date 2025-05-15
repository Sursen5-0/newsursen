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
    public class DealConfiguration : BaseEntityConfiguration<Deal>
    {
        public override void Configure(EntityTypeBuilder<Deal> builder)
        {
            BaseConfigure(builder);

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id).ValueGeneratedNever();
            builder.Property(e => e.CustomerName).HasMaxLength(255);
            builder.Property(e => e.Description).HasMaxLength(1000);
            builder.Property(e => e.ExternalId).HasMaxLength(255);
            builder.Property(e => e.Name).HasMaxLength(255);
            builder.Property(e => e.Probability).HasColumnType("decimal(5, 2)");

            builder.HasOne(d => d.Responsible)
                .WithMany(p => p.Deals).HasForeignKey(d => d.ResponsibleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
