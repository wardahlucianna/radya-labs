using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsHomeroomStudentEnrollment : AuditEntity, ISchedulingEntity
    {
        public string IdHomeroomStudent { get; set; }
        public string IdLesson { get; set; }
        //public int Semester { get; set; }
        public string IdSubject { get; set; }
        public string IdSubjectLevel { get; set; }
        public bool IsFromMaster { get; set; }

        public virtual MsHomeroomStudent HomeroomStudent { get; set; }
        public virtual MsLesson Lesson { get; set; }
        public virtual ICollection<TrAscTimetableEnrollment> AscTimetableEnrollments { get; set; }
        public virtual MsSubject Subject { get; set; }
        public virtual MsSubjectLevel SubjectLevel { get; set; }
        public virtual ICollection<TrHomeroomStudentEnrollment> TrHomeroomStudentEnrollments { get; set; }
        //public virtual ICollection<HMsHomeroomStudentEnrollment> HMsHomeroomStudentEnrollments { get; set; }

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
                .HasMaxLength(36);

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

            builder.HasOne(x => x.SubjectLevel)
                .WithMany(x => x.HomeroomStudentEnrollments)
                .HasForeignKey(fk => fk.IdSubjectLevel)
                .HasConstraintName("FK_MsHomeroomStudentEnrollment_MsSubjectLevel")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
