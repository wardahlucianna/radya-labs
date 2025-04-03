using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrGeneratedScheduleGrade : AuditEntity, ISchedulingEntity
    {
        public string IdGrade { get; set; }
        public string IdGeneratedSchedule { get; set; }
        public DateTime StartPeriod { get; set; }
        public DateTime EndPeriod { get; set; }

        public virtual TrGeneratedSchedule GeneratedSchedule { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual ICollection<TrGeneratedScheduleStudent> GeneratedScheduleStudents { get; set; }
        public virtual ICollection<MsScheduleLesson> ScheduleLessons { get; set; }
    }

    internal class TrGeneratedScheduleGradeConfiguration : AuditEntityConfiguration<TrGeneratedScheduleGrade>
    {
        public override void Configure(EntityTypeBuilder<TrGeneratedScheduleGrade> builder)
        {
            builder.HasOne(x => x.Grade)
               .WithMany(x => x.GeneratedScheduleGrades)
               .HasForeignKey(fk => fk.IdGrade)
               .HasConstraintName("FK_TrGeneratedScheduleGrade_MsGrade")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.Property(x => x.StartPeriod)
               .IsRequired();

            builder.Property(x => x.EndPeriod)
                .IsRequired();

            builder.HasOne(x => x.GeneratedSchedule)
                .WithMany(x => x.GeneratedScheduleGrades)
                .HasForeignKey(fk => fk.IdGeneratedSchedule)
                .HasConstraintName("FK_TrGeneratedScheduleGrade_TrGeneratedSchedule")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
