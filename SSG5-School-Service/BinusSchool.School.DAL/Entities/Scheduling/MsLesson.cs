using System.Collections.Generic;
using BinusSchool.Persistence.Abstractions;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities.Scheduling
{
    public class MsLesson : AuditEntity, ISchoolEntity
    {
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public string IdSubject { get; set; }
        public string ClassIdGenerated { get; set; }
        public int Semester { get; set; }
        public int TotalPerWeek { get; set; }
        public string HomeroomSelected { get; set; }
        
        public virtual MsGrade Grade { get; set; }
        public virtual MsSubject Subject { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<TrPublishSurveyMapping> PublishSurveyMappings { get; set; }
        public virtual ICollection<MsHomeroomStudentEnrollment> HomeroomStudentEnrollments { get; set; }
        public virtual ICollection<TrScheduleRealization2> ScheduleRealization2s { get; set; }
        public virtual ICollection<MsLessonTeacher> LessonTeachers { get; set; }


    }

    internal class MsLessonConfiguration : AuditEntityConfiguration<MsLesson>
    {
        public override void Configure(EntityTypeBuilder<MsLesson> builder)
        {
          
            builder.Property(x => x.IdGrade)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdSubject)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.ClassIdGenerated)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.Semester)
                .IsRequired();


            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.Lessons)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsLesson_MsAcademicYear")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.Lessons)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsLesson_MsGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();


            builder.HasOne(x => x.Subject)
                .WithMany(x => x.Lessons)
                .HasForeignKey(fk => fk.IdSubject)
                .HasConstraintName("FK_MsLesson_MsSubject")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            base.Configure(builder);
        }
    }
}
