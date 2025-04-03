using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Schedulling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.Scheduling
{
    public class MsLesson : AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public string IdSubject { get; set; }
        public string ClassIdFormat { get; set; }
        public string ClassIdExample { get; set; }
        public string ClassIdGenerated { get; set; }
        public int Semester { get; set; }
        public int TotalPerWeek { get; set; }
        public string HomeroomSelected { get; set; }

        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual MsSubject Subject { get; set; }
        public virtual ICollection<MsLessonTeacher> LessonTeachers { get; set; }
        public virtual ICollection<MsHomeroomStudentEnrollment> HomeroomStudentEnrollments { get; set; }
        public virtual ICollection<MsLessonPathway> LessonPathways { get; set; }
    }

    internal class MsLessonConfiguration : AuditEntityConfiguration<MsLesson>
    {
        public override void Configure(EntityTypeBuilder<MsLesson> builder)
        {
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

            builder.Property(x => x.ClassIdFormat)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(x => x.ClassIdExample)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(x => x.ClassIdGenerated)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.Semester)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
