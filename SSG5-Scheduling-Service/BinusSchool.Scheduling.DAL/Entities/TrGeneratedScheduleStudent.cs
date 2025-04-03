using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrGeneratedScheduleStudent : AuditEntity, ISchedulingEntity
    {
        public string IdStudent { get; set; }
        public string IdGenerateScheduleGrade { get; set; }

        public virtual ICollection<TrGeneratedScheduleLesson> GeneratedScheduleLessons { get; set; }
        public virtual TrGeneratedScheduleGrade GeneratedScheduleGrade { get; set; }
        public virtual MsStudent Student { get; set; }
    }

    internal class TrGeneratedScheduleStudentConfiguration : AuditEntityConfiguration<TrGeneratedScheduleStudent>
    {
        public override void Configure(EntityTypeBuilder<TrGeneratedScheduleStudent> builder)
        {

            builder.HasOne(x => x.Student)
                .WithMany(x => x.GeneratedScheduleStudents)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_TrGeneratedScheduleStudent_MsStudent")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();


            builder.Property(x => x.IdGenerateScheduleGrade)
              .HasMaxLength(36)
              .IsRequired(); 

            builder.HasOne(x => x.GeneratedScheduleGrade)
                .WithMany(x => x.GeneratedScheduleStudents)
                .HasForeignKey(fk => fk.IdGenerateScheduleGrade)
                .HasConstraintName("FK_TrGeneratedScheduleStudent_TrGeneratedScheduleGrade")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            base.Configure(builder);
        }
    }
}
