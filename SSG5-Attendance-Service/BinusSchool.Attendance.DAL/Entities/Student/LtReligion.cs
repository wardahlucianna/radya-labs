using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Student
{
    public class LtReligion : AuditEntity, IAttendanceEntity
    {
        public string ReligionName { get; set; }
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
