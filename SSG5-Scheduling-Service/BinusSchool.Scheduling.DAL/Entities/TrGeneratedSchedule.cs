using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Domain.Abstractions;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrGeneratedSchedule : AuditEntity, ISchedulingEntity
    {
        public string IdAscTimetable { get; set; }

        public virtual TrAscTimetable TrAscTimetable { get; set; }
        public virtual ICollection<TrGeneratedScheduleGrade> GeneratedScheduleGrades { get; set; }
    }

    internal class TrGeneratedScheduleConfiguration : AuditEntityConfiguration<TrGeneratedSchedule>
    {
        public override void Configure(EntityTypeBuilder<TrGeneratedSchedule> builder)
        {
            builder.Property(x => x.IdAscTimetable)
               .HasMaxLength(36)
               .IsRequired();

            builder.HasOne(x => x.TrAscTimetable)
                .WithMany(x => x.GeneratedSchedules)
                .HasForeignKey(fk => fk.IdAscTimetable)
                .HasConstraintName("FK_TrGeneratedSchedule_TrAscTimetable")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
