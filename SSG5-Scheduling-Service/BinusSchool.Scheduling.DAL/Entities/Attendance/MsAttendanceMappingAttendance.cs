using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsAttendanceMappingAttendance : AuditEntity, ISchedulingEntity
    {
        public string IdMappingAttendance {get;set;}
        public string IdAttendance {get;set;}

        public virtual MsAttendance Attendance {get;set;}
        public virtual MsMappingAttendance MappingAttendance {get;set;}
        public virtual ICollection<TrAttendanceEntry> AttendanceEntries {get;set;}
    }

    internal class MsAttendanceMappingAttendanceConfiguration : AuditEntityConfiguration<MsAttendanceMappingAttendance>
    {
        public override void Configure(EntityTypeBuilder<MsAttendanceMappingAttendance> builder)
        {
            builder.HasOne(x => x.Attendance)
                .WithMany(x => x.AttendanceMappingAttendances)
                .HasForeignKey(fk => fk.IdAttendance)
                .HasConstraintName("FK_MsAttendanceMappingAttendance_MsAttendance")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.MappingAttendance)
                .WithMany(x => x.AttendanceMappingAttendances)
                .HasForeignKey(fk => fk.IdMappingAttendance)
                .HasConstraintName("FK_MsAttendanceMappingAttendance_MsMappingAttendace")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
