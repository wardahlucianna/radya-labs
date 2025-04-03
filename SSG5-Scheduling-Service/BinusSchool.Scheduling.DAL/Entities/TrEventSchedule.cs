using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrEventSchedule : AuditEntity, ISchedulingEntity
    {
        public string IdEvent { get; set; }
        public string IdScheduleLesson { get; set; }
        public bool IsSyncAttendance { get; set; }
        public DateTime? DateSyncAttendance { get; set; }
        public virtual TrEvent Event { get; set; }
        public virtual MsScheduleLesson ScheduleLesson { get; set; }
    }

    internal class TrEventScheduleConfiguration : AuditEntityConfiguration<TrEventSchedule>
    {
        public override void Configure(EntityTypeBuilder<TrEventSchedule> builder)
        {
            builder.HasOne(x => x.Event)
                .WithMany(x => x.EventSchedules)
                .HasForeignKey(fk => fk.IdEvent)
                .HasConstraintName("FK_TrEventSchedule_TrEvent")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

             builder.HasOne(x => x.ScheduleLesson)
                .WithMany(x => x.EventSchedules)
                .HasForeignKey(fk => fk.IdScheduleLesson)
                .HasConstraintName("FK_TrEventSchedule_MsScheduleLesson")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
