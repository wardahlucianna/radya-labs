using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class TrAttendanceSummaryMappingAtd : AuditEntity, IAttendanceEntity
    {
        public string IdAttendanceSummary { get; set; }
        public string IdAttendance { get; set; }
        public int CountAsDay { get; set; }
        public int CountAsSession { get; set; }

        public virtual TrAttendanceSummary AttendanceSummary { get; set; }
        public virtual MsAttendance Attendance { get; set; }
        
    }

    internal class TrAttendanceSummaryMappingAtdConfiguration : AuditEntityConfiguration<TrAttendanceSummaryMappingAtd>
    {
        public override void Configure(EntityTypeBuilder<TrAttendanceSummaryMappingAtd> builder)
        {
            builder.HasOne(x => x.AttendanceSummary)
               .WithMany(x => x.AttendanceSummaryMappingAtds)
               .HasForeignKey(fk => fk.IdAttendanceSummary)
               .HasConstraintName("FK_TrAttendanceSummaryMappingAtd_TrAttendanceSummary")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

            builder.HasOne(x => x.Attendance)
               .WithMany(x => x.AttendanceSummaryMappingAtds)
               .HasForeignKey(fk => fk.IdAttendance)
               .HasConstraintName("FK_TrAttendanceSummaryMappingAtd_MsAttendance")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

            base.Configure(builder);
        }
    }

}
