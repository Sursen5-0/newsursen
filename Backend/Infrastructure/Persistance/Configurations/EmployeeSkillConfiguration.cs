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
    public class EmployeeSkillConfiguration : BaseEntityConfiguration<EmployeeSkill>
    {
        public override void Configure(EntityTypeBuilder<EmployeeSkill> builder)
        {
            BaseConfigure(builder);

            builder.HasKey(e => e.Id);

            builder.Property(e => e.YearsExperience).HasColumnType("decimal(3, 1)");

            builder.Property(e => e.Name).HasMaxLength(255);
            builder.Property(e => e.ExternalId).HasMaxLength(24).IsFixedLength().IsUnicode(false);

            builder.HasOne(d => d.Employee)
                .WithMany()
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.Skill)
                .WithMany()
                .HasForeignKey(d => d.SkillId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
