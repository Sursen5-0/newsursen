using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Persistance.Models;

namespace Infrastructure.Persistance.Configurations
{
    public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.Property(e => e.CreatedAt)
                   .HasDefaultValueSql("getdate()")
                   .HasColumnType("datetime");

            builder.Property(e => e.UpdatedAt)
                   .HasDefaultValueSql("getdate()")
                   .HasColumnType("datetime");
        }
    }
}
