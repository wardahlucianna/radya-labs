using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Abstractions;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrHomeroomStudentEnrollment : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string IdTrHomeroomStudentEnrollment { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string IdHomeroomStudentEnrollment { get; set; }
        public DateTime StartDate { get; set; }
        public string Note { get; set; }
        public bool IsSendEmail { get; set; }
        public string IdLessonNew { get; set; }
        public string IdSubjectNew { get; set; }
        public string IdSubjectLevelNew { get; set; }
        public string IdLessonOld { get; set; }
        public string IdSubjectOld { get; set; }
        public string IdSubjectLevelOld { get; set; }
        public bool IsShowHistory { get; set; }
        public bool IsDelete { get; set; }
        public bool? IsSync { get; set; }
        public DateTime? DateSync { get; set; }
        public virtual MsHomeroomStudent HomeroomStudent { get; set; }
        public virtual MsHomeroomStudentEnrollment HomeroomStudentEnrollment { get; set; }
        public virtual MsLesson LessonNew { get; set; }
        public virtual MsSubject SubjectNew { get; set; }
        public virtual MsSubjectLevel SubjectLevelNew { get; set; }
        public virtual MsLesson LessonOld { get; set; }
        public virtual MsSubject SubjectOld { get; set; }
        public virtual MsSubjectLevel SubjectLevelOld { get; set; }
        //public virtual ICollection<HMsHomeroomStudentEnrollment> HMsHomeroomStudentEnrollments { get; set; }

    }

    internal class TrHomeroomStudentEnrollmentConfiguration : AuditNoUniqueEntityConfiguration<TrHomeroomStudentEnrollment>
    {
        public override void Configure(EntityTypeBuilder<TrHomeroomStudentEnrollment> builder)
        {
            builder.HasKey(x => x.IdTrHomeroomStudentEnrollment);
            builder.Property(p => p.IdTrHomeroomStudentEnrollment).HasMaxLength(36).IsRequired();

            builder.HasOne(x => x.HomeroomStudent)
              .WithMany(x => x.TrHomeroomStudentEnrollments)
              .HasForeignKey(fk => fk.IdHomeroomStudent)
              .HasConstraintName("FK_TrHomeroomStudentEnrollment_MsHomeroomStudent")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.HomeroomStudentEnrollment)
              .WithMany(x => x.TrHomeroomStudentEnrollments)
              .HasForeignKey(fk => fk.IdHomeroomStudentEnrollment)
              .HasConstraintName("FK_TrHomeroomStudentEnrollment_MsHomeroomStudentEnrollment")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.LessonNew)
              .WithMany(x => x.TrHomeroomStudentEnrollmentsNews)
              .HasForeignKey(fk => fk.IdLessonNew)
              .HasConstraintName("FK_MsHomeroomStudentEnrollment_MsLessonNew")
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.LessonOld)
              .WithMany(x => x.TrHomeroomStudentEnrollmentsOlds)
              .HasForeignKey(fk => fk.IdLessonOld)
              .HasConstraintName("FK_MsHomeroomStudentEnrollment_MsLessonOld")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.SubjectNew)
              .WithMany(x => x.TrHomeroomStudentEnrollmentsNews)
              .HasForeignKey(fk => fk.IdSubjectNew)
              .HasConstraintName("FK_MsHomeroomStudentEnrollment_MsSubjectNew")
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.SubjectOld)
             .WithMany(x => x.TrHomeroomStudentEnrollmentsOlds)
             .HasForeignKey(fk => fk.IdSubjectOld)
             .HasConstraintName("FK_MsHomeroomStudentEnrollment_MsSubjectOld")
             .OnDelete(DeleteBehavior.NoAction)
             .IsRequired();

            builder.HasOne(x => x.SubjectLevelNew)
              .WithMany(x => x.TrHomeroomStudentEnrollmentsNews)
              .HasForeignKey(fk => fk.IdSubjectLevelNew)
              .HasConstraintName("FK_MsHomeroomStudentEnrollment_MsSubjectLevelNew")
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.SubjectLevelOld)
              .WithMany(x => x.TrHomeroomStudentEnrollmentsOlds)
              .HasForeignKey(fk => fk.IdSubjectLevelOld)
              .HasConstraintName("FK_MsHomeroomStudentEnrollment_MsSubjectLevelOld")
              .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
