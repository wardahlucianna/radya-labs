using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsWeekVariant : CodeEntity, ISchedulingEntity
    {
        public virtual ICollection<MsLesson> Lessons { get; set; }
        public virtual ICollection<MsWeekVariantDetail> WeekVarianDetails { get; set; }
    }

    internal class MsWeekVariantConfiguration : CodeEntityConfiguration<MsWeekVariant>
    {
        public override void Configure(EntityTypeBuilder<MsWeekVariant> builder)
        {
            base.Configure(builder);
        }
    }
}
