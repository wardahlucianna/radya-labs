using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities.Student
{
    public class LtReligion : AuditEntity, ISchoolEntity
    {
        public string ReligionName { get; set; }
        public virtual ICollection<MsStudent> Students { get; set; }
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
