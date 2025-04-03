using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.Scheduling
{
    public class MsLessonTeacher : AuditEntity, IStudentEntity
    {
        public string IdLesson { get; set; }
        public string IdUser { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsAttendance { get; set; }
        public bool IsScore { get; set; }

        public virtual MsLesson Lesson { get; set; }
        public virtual MsStaff Staff { get; set; }
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
