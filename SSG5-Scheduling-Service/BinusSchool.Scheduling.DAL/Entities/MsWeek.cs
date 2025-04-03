using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsWeek : CodeEntity, ISchedulingEntity
    {

        public virtual ICollection<MsWeekVariantDetail> WeekVarianDetails { get; set; }
        public virtual ICollection<TrGeneratedScheduleLesson> GeneratedScheduleLessons { get; set; }
        public virtual ICollection<MsScheduleLesson> ScheduleLesson { get; set; }
        public virtual ICollection<MsSchedule> Schedules { get; set; }

    }

    internal class MsWeekConfiguration : CodeEntityConfiguration<MsWeek>
    {
        public override void Configure(EntityTypeBuilder<MsWeek> builder)
        {

            base.Configure(builder);
        }
    }
}
