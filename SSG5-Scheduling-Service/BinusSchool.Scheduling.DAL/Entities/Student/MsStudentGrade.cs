using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.Student
{
    public class MsStudentGrade : AuditEntity, ISchedulingEntity
    {
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }

        public virtual MsStudent Student { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual ICollection<MsStudentGradePathway> StudentGradePathways { get; set; }
        public virtual ICollection<TrAttendanceAdministration> AttendanceAdministrations { get; set; }
    }

    internal class MsStudentGradeConfiguration : AuditEntityConfiguration<MsStudentGrade>
    {
        public override void Configure(EntityTypeBuilder<MsStudentGrade> builder)
        {
            builder.HasOne(x => x.Student)
                .WithMany(x => x.StudentGrades)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_MsStudentGrade_MsStudent")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.StudentGrades)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsStudentGrade_MsGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
