using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Scheduling
{
    public class TrGeneratedScheduleStudent : AuditEntity, IAttendanceEntity
    {
        public string IdStudent { get; set; }

        public virtual MsStudent Student { get; set; }
        public virtual ICollection<TrGeneratedScheduleLesson> GenerateScheduleLessons { get; set; }
    }

    internal class TrGenerateScheduleStudentConfiguration : AuditEntityConfiguration<TrGeneratedScheduleStudent>
    {
        public override void Configure(EntityTypeBuilder<TrGeneratedScheduleStudent> builder)
        {
            builder.HasOne(x => x.Student)
                .WithMany(x => x.GenerateScheduleStudents)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_TrGenerateScheduleStudent_MsStudent")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }

}
