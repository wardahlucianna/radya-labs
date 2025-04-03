using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.School
{
    public class MsSubject : CodeEntity, ISchedulingEntity
    {
        public string IdGrade { get; set; }
        public string IdDepartment { get; set; }
        public string SubjectID { get; set; }
        public int MaxSession { get; set; }
        public string IdSubjectGroup { get; set; }
        public string IdCurriculum { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual MsDepartment Department { get; set; }
        public virtual MsSubjectGroup SubjectGroup { get; set; }
        public virtual MsCurriculum Curriculum { get; set; }
        public virtual ICollection<MsHomeroomStudentEnrollment> HomeroomStudentEnrollments { get; set; }
        public virtual ICollection<MsLesson> Lessons { get; set; }
        public virtual ICollection<MsSubjectMappingSubjectLevel> SubjectMappingSubjectLevels { get; set; }
        public virtual ICollection<TrGeneratedScheduleLesson> GeneratedScheduleLessons { get; set; }
        public virtual ICollection<MsScheduleLesson> ScheduleLesson { get; set; }
        public virtual ICollection<TrHomeroomStudentEnrollment> TrHomeroomStudentEnrollmentsNews { get; set; }
        public virtual ICollection<TrHomeroomStudentEnrollment> TrHomeroomStudentEnrollmentsOlds { get; set; }
        public virtual ICollection<TrInvitationBookingSettingExcludeSub> InvBookingSettingExcludeSub { get; set; }
    }

    internal class MsSubjectConfiguration : CodeEntityConfiguration<MsSubject>
    {
        public override void Configure(EntityTypeBuilder<MsSubject> builder)
        {
            builder.Property(x => x.SubjectID)
                .HasMaxLength(50)
                .IsRequired();


            builder.HasOne(x => x.Curriculum)
                .WithMany(x => x.Subjects)
                .HasForeignKey(fk => fk.IdCurriculum)
                .HasConstraintName("FK_MsSubject_MsCurriculum")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.Subjects)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsSubject_MsGrade")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.Department)
                .WithMany(x => x.Subjects)
                .HasForeignKey(fk => fk.IdDepartment)
                .HasConstraintName("FK_MsSubject_MsDepartment")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.SubjectGroup)
               .WithMany(x => x.Subjects)
               .HasForeignKey(fk => fk.IdSubjectGroup)
               .HasConstraintName("FK_MsSubject_MsSubjectGroup")
               .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}
