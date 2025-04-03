using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Scheduling
{
    public class TrEventIntendedForGradeStudent : AuditEntity, IAttendanceEntity
    {
        public string IdHomeroom { get; set; }
        public string IdEventIntendedFor { get; set; }

        public virtual TrEventIntendedFor EventIntendedFor { get; set; }
        public virtual MsHomeroom Homeroom { get; set; }
    }

    internal class TrEventIntendedForGradeStudentConfiguration : AuditEntityConfiguration<TrEventIntendedForGradeStudent>
    {
        public override void Configure(EntityTypeBuilder<TrEventIntendedForGradeStudent> builder)
        {
            builder.HasOne(x => x.EventIntendedFor)
              .WithMany(x => x.EventIntendedForGradeStudents)
              .HasForeignKey(fk => fk.IdEventIntendedFor)
              .HasConstraintName("FK_TrEventIntendedForGradeStudent_TrEventIntendedFor")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.Homeroom)
             .WithMany(x => x.TrEventIntendedForGradeStudents)
             .HasForeignKey(fk => fk.IdHomeroom)
             .HasConstraintName("FK_TrEventIntendedForGradeStudent_MsHomeroom")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();


            base.Configure(builder);
        }

    }
}
