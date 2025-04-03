using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.Student
{
    public class LtReligion : AuditEntity, ISchedulingEntity
    {
        public string ReligionName { get; set; }

        public virtual ICollection<MsStudent> Student { get; set; }
    }

    internal class LtReligionConfiguration : AuditEntityConfiguration<LtReligion>
    {
        public override void Configure(EntityTypeBuilder<LtReligion> builder)
        {   
            builder.Property(x => x.ReligionName)                
                .HasMaxLength(36);
          
            base.Configure(builder);
        }

    }
}
