using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class MsWorkhabit : CodeEntity, IAttendanceEntity
    {
        public string IdAcademicYear { get; set; }

        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<MsMappingAttendanceWorkhabit> MappingAttendanceWorkhabits { get; set; }
        public virtual ICollection<TrAttendanceSummaryWorkhabit> AttendanceSummaryWorkhabits { get; set; }
    }

    internal class MsWorkhabitConfiguration : CodeEntityConfiguration<MsWorkhabit>
    {
        public override void Configure(EntityTypeBuilder<MsWorkhabit> builder)
        {
            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.Workhabits)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsWorkhabit_MsAcademicYear")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
