using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Teaching
{
    public class LtPosition : CodeEntity, IAttendanceEntity
    {
        public virtual ICollection<MsTeacherPosition> TeacherPositions { get; set; }
    }

    internal class LtPositionConfiguration : CodeEntityConfiguration<LtPosition>
    {
        public override void Configure(EntityTypeBuilder<LtPosition> builder)
        {
            base.Configure(builder);
        }
    }
}
