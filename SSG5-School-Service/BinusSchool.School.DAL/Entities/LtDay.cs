using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class LtDay : CodeEntity, ISchoolEntity
    {
        public virtual ICollection<MsSession> Sessions { get; set; }
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
