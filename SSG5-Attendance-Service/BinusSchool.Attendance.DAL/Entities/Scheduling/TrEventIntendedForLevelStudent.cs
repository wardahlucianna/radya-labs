using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Scheduling
{
    public class TrEventIntendedForLevelStudent : AuditEntity, IAttendanceEntity
    {
        public string IdLevel { get; set; }
        public string IdEventIntendedFor { get; set; }

        public virtual TrEventIntendedFor EventIntendedFor { get; set; }
        public virtual MsLevel Level { get; set; }
    }

    internal class TrEventIntendedForLevelStudentConfiguration : AuditEntityConfiguration<TrEventIntendedForLevelStudent>
    {
        public override void Configure(EntityTypeBuilder<TrEventIntendedForLevelStudent> builder)
        {
            builder.HasOne(x => x.EventIntendedFor)
             .WithMany(x => x.EventIntendedForLevelStudents)
             .HasForeignKey(fk => fk.IdEventIntendedFor)
             .HasConstraintName("FK_TrEventIntendedForLevelStudent_TrEventIntendedFor")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            builder.HasOne(x => x.Level)
             .WithMany(x => x.EventIntendedForLevelStudents)
             .HasForeignKey(fk => fk.IdLevel)
             .HasConstraintName("FK_TrEventIntendedForLevelStudent_MsLevel")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
