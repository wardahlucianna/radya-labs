using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Schedulling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.Scheduling
{
    public class TrAscTimetableEnrollment : AuditEntity, IStudentEntity
    {
        public string IdHomeroomStudentEnrollment { get; set; }
        public string IdAscTimetable { get; set; }

        public virtual MsHomeroomStudentEnrollment HomeroomStudentEnrollment { get; set; }
        public virtual TrAscTimetable AscTimetable { get; set; }
    }

    internal class TrAscTimetableEnrollmentConfiguration : AuditEntityConfiguration<TrAscTimetableEnrollment>
    {
        public override void Configure(EntityTypeBuilder<TrAscTimetableEnrollment> builder)
        {
            builder.HasOne(x => x.AscTimetable)
                .WithMany(x => x.AscTimetableEnrollments)
                .HasForeignKey(fk => fk.IdAscTimetable)
                .HasConstraintName("FK_TrAscTimetableEnrollment_TrAscTimetable")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.HomeroomStudentEnrollment)
                .WithMany(x => x.AscTimetableEnrollments)
                .HasForeignKey(fk => fk.IdHomeroomStudentEnrollment)
                .HasConstraintName("FK_TrAscTimetableEnrollment_MsHomeroomStudentEnrollment")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
