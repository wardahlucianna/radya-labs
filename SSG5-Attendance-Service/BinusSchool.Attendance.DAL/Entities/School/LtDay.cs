using System.Collections.Generic;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.School
{
    public class LtDay : CodeEntity, IAttendanceEntity
    {
        public virtual ICollection<MsSession> Sessions { get; set; }
        public virtual ICollection<MsSchedule> Schedules { get; set; }
        public virtual ICollection<MsScheduleLesson> ScheduleLesson { get; set; }
        public virtual ICollection<TrScheduleRealization2> ScheduleRealization2s { get; set; }

    }

    internal class LtDayConfiguration : CodeEntityConfiguration<LtDay>
    {
        public override void Configure(EntityTypeBuilder<LtDay> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Code)
                .HasMaxLength(36);

            builder.Property(x => x.Description)
                .HasMaxLength(36);
        }
    }
}
