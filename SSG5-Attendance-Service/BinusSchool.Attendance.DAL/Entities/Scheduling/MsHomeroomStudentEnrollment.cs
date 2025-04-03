using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.AttendanceDb.Entities.School;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Scheduling
{
    public class MsHomeroomStudentEnrollment : AuditEntity, IAttendanceEntity
    {
        public string IdHomeroomStudent { get; set; }
        public string IdLesson { get; set; }
        //public int Semester { get; set; }
        public string IdSubject { get; set; }
        public string IdSubjectLevel { get; set; }
        public bool IsFromMaster { get; set; }

        public virtual MsHomeroomStudent HomeroomStudent { get; set; }
        public virtual MsLesson Lesson { get; set; }
        public virtual MsSubject Subject { get; set; }
        public virtual ICollection<TrHomeroomStudentEnrollment> TrHomeroomStudentEnrollments { get; set; }
    }

    internal class MsHomeroomStudentEnrollmentConfiguration : AuditEntityConfiguration<MsHomeroomStudentEnrollment>
    {
        public override void Configure(EntityTypeBuilder<MsHomeroomStudentEnrollment> builder)
        {

            builder.Property(x => x.IsFromMaster)
              .HasDefaultValue(false);

            builder.HasOne(x => x.Subject)
              .WithMany(x => x.HomeroomStudentEnrollments)
              .HasForeignKey(fk => fk.IdSubject)
              .HasConstraintName("FK_MsHomeroomStudentEnrollment_MsSubject")
              .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.Property(x => x.IdSubjectLevel)
                .HasMaxLength(36)
                .HasDefaultValue(false);

            builder.HasOne(x => x.HomeroomStudent)
                .WithMany(x => x.HomeroomStudentEnrollments)
                .HasForeignKey(fk => fk.IdHomeroomStudent)
                .HasConstraintName("FK_MsHomeroomStudentEnrollment_MsHomeroomStudent")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Lesson)
                .WithMany(x => x.HomeroomStudentEnrollments)
                .HasForeignKey(fk => fk.IdLesson)
                .HasConstraintName("FK_MsHomeroomStudentEnrollment_MsLesson")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
