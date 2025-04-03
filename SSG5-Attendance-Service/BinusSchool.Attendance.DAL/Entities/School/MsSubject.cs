using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.School
{
    public class MsSubject : CodeEntity, IAttendanceEntity
    {
        public string IdGrade { get; set; }
        public string IdDepartment { get; set; }
        //public string IdSubjectGroup { get; set; }
        public string SubjectID { get; set; }
        public int MaxSession { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual MsDepartment Department { get; set; }
        //public virtual MsSubjectGroup SubjectGroup { get; set; }
        public virtual ICollection<MsLesson> Lessons { get; set; }
        //public virtual ICollection<MsEventIntendedForSubject> EventIntendedForSubjects { get; set; }
        public virtual ICollection<TrGeneratedScheduleLesson> GeneratedScheduleLessons { get; set; }
        public virtual ICollection<MsHomeroomStudentEnrollment> HomeroomStudentEnrollments { get; set; }
        //public virtual ICollection<MsEventIntendedForSubjectStudent> EventIntendedForSubjectStudents { get; set; }
        public virtual ICollection<MsScheduleLesson> ScheduleLesson { get; set; }
        public virtual ICollection<TrHomeroomStudentEnrollment> TrHomeroomStudentEnrollmentsNews { get; set; }
        public virtual ICollection<TrHomeroomStudentEnrollment> TrHomeroomStudentEnrollmentsOlds { get; set; }


    }

    internal class MsSubjectConfiguration : CodeEntityConfiguration<MsSubject>
    {
        public override void Configure(EntityTypeBuilder<MsSubject> builder)
        {
            builder.Property(x => x.SubjectID)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.IdDepartment)
                .HasMaxLength(36);

            builder.HasOne(x => x.Department)
              .WithMany(x => x.Subjects)
              .HasForeignKey(fk => fk.IdDepartment)
              .HasConstraintName("FK_MsSubject_MsDepartment")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.Subjects)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsSubject_MsGrade")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            //builder.HasOne(x => x.SubjectGroup)
            //    .WithMany(x => x.Subjects)
            //    .HasForeignKey(fk => fk.IdSubjectGroup)
            //    .HasConstraintName("FK_MsSubject_MsSubjectGroup")
            //    .OnDelete(DeleteBehavior.Restrict);
            base.Configure(builder);
        }
    }
}
