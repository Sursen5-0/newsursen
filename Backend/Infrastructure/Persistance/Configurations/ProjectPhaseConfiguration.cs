using Infrastructure.Persistance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Configurations
{
    public class ProjectPhaseConfiguration : BaseEntityConfiguration<ProjectPhase>
    {
        public override void Configure(EntityTypeBuilder<ProjectPhase> builder)
        {
            BaseConfigure(builder);

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id).ValueGeneratedNever();
            builder.Property(e => e.Name)
                .HasMaxLength(255);
            builder.HasOne(d => d.ParentPhase).WithMany(p => p.UnderPhases)
                .HasForeignKey(d => d.ParentPhaseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Project).WithMany(p => p.Phases)
                    .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}