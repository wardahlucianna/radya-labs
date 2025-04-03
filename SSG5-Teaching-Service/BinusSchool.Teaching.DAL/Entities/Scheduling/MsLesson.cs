using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities.Scheduling
{
    public class MsLesson : AuditEntity, ITeachingEntity
    {
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public string IdSubject { get; set; }
        public int Semester { get; set; }
        public string ClassIdGenerated { get; set; }
        public virtual MsSubject Subject { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual ICollection<MsLessonTeacher> LessonTeachers { get; set; }
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
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.HasOne(x => x.Grade)
            .WithMany(x => x.Lessons)
            .HasForeignKey(fk => fk.IdGrade)
            .HasConstraintName("FK_MsLesson_MsGrade")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.HasOne(x => x.Subject)
            .WithMany(x => x.Lessons)
            .HasForeignKey(fk => fk.IdSubject)
            .HasConstraintName("FK_MsLesson_MsSubject")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            base.Configure(builder);

            builder.Property(x => x.ClassIdGenerated)
                .HasMaxLength(255)
                .IsRequired();

        }
    }
}
