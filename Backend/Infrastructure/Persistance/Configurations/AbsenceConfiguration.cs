using Infrastructure.Persistance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.Configurations
{
    public class AbsenceConfiguration : BaseEntityConfiguration<Absence>
    {
        public void Configure(EntityTypeBuilder<Absence> builder)
        {
            base.Configure(builder);
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id).ValueGeneratedNever();

            builder.HasOne(d => d.Employee).WithMany(p => p.Absences)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}