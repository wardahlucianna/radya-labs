using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class LtEmergencyStatus : AuditEntity, IAttendanceEntity
    {
        public string EmergencyStatusName { get; set; }
        public string ColorCode { get; set; }
        public virtual ICollection<TrEmergencyAttendance> EmergencyAttendances { get; set; }
        public virtual ICollection<HTrEmergencyAttendance> HistoryEmergencyAttendances { get; set; }
    }
    internal class MsEmergencyStatusConfiguration : AuditEntityConfiguration<LtEmergencyStatus>
    {
        public override void Configure(EntityTypeBuilder<LtEmergencyStatus> builder)
        {

            builder.Property(x => x.EmergencyStatusName)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.ColorCode)
               .HasMaxLength(10);

            base.Configure(builder);
        }
    }
}
