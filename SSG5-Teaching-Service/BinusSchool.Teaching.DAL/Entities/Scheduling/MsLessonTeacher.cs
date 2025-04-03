using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities.Scheduling
{
    public class MsLessonTeacher : AuditEntity, ITeachingEntity
    {
        public string IdLesson { get; set; }
        public string IdUser { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsAttendance { get; set; }
        public bool IsScore { get; set; }
        public bool IsClassDiary { get; set; }
        public bool IsLessonPlan { get; set; }
        public virtual MsLesson Lesson { get; set; }
        public virtual MsStaff Staff { get; set; }
        public virtual ICollection<TrLessonPlan> LessonPlans { get; set; }
    }

    internal class MsLessonTeacherConfiguration : AuditEntityConfiguration<MsLessonTeacher>
    {
        public override void Configure(EntityTypeBuilder<MsLessonTeacher> builder)
        {
            builder.HasOne(x => x.Staff)
                .WithMany(x => x.LessonTeachers)
                .HasForeignKey(fk => fk.IdUser)
                .HasConstraintName("FK_MsLessonTeacher_MsStaff")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Lesson)
                .WithMany(x => x.LessonTeachers)
                .HasForeignKey(fk => fk.IdLesson)
                .HasConstraintName("FK_MsLessonTeacher_MsLesson")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
