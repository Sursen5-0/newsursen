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
    public class SkillConfiguration : BaseEntityConfiguration<Skill>
    {
        public override void Configure(EntityTypeBuilder<Skill> builder)
        {
            BaseConfigure(builder);

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id).ValueGeneratedNever();
            builder.Property(e => e.Name)
                .HasMaxLength(255);
        }
    }
}