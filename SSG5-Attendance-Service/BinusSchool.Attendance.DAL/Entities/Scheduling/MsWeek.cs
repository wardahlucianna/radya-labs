using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Scheduling
{
    public class MsWeek : CodeEntity, IAttendanceEntity
    {
        public virtual ICollection<TrGeneratedScheduleLesson> GeneratedScheduleLessons { get; set; }
        public virtual ICollection<MsSchedule> Schedules { get; set; }
        public virtual ICollection<MsScheduleLesson> ScheduleLesson { get; set; }

    }

    internal class MsWeekConfiguration : CodeEntityConfiguration<MsWeek>
    {
        public override void Configure(EntityTypeBuilder<MsWeek> builder)
        {
            base.Configure(builder);
        }
    }
}
