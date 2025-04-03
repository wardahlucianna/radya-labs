using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class TrAttendanceEntryWorkhabitV2 : AuditEntity, IAttendanceEntity
    {
        public string IdAttendanceEntry { get; set; }
        public string IdMappingAttendanceWorkhabit { get; set; }

        public virtual TrAttendanceEntryV2 AttendanceEntry { get; set; }
        public virtual MsMappingAttendanceWorkhabit MappingAttendanceWorkhabit { get; set; }
    }

    internal class TrAttendanceEntryWorkhabitV2Configuration : AuditEntityConfiguration<TrAttendanceEntryWorkhabitV2>
    {
        public override void Configure(EntityTypeBuilder<TrAttendanceEntryWorkhabitV2> builder)
        {
            builder.HasOne(x => x.AttendanceEntry)
               .WithMany(x => x.AttendanceEntryWorkhabitV2s)
               .HasForeignKey(fk => fk.IdAttendanceEntry)
               .HasConstraintName("FK_TrAttendanceEntryWorkhabitV2_TrAttendance")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

            builder.HasOne(x => x.MappingAttendanceWorkhabit)
                .WithMany(x => x.AttendanceEntryWorkhabitV2s)
                .HasForeignKey(fk => fk.IdMappingAttendanceWorkhabit)
                .HasConstraintName("FK_TrAttendanceEntryWorkhabitV2_MsMappingAttendanceWorkhabit")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
