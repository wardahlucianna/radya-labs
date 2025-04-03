using System;
using System.Collections;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class TrAttendanceEntry : AuditEntity, IAttendanceEntity
    {
        public string IdGeneratedScheduleLesson {get;set;}
        public string IdAttendanceMappingAttendance {get;set;}
        public TimeSpan? LateTime {get;set;}
        public string FileEvidence {get;set;}
        public string Notes {get;set;}
        public AttendanceEntryStatus Status {get;set;}
        public bool IsFromAttendanceAdministration {get;set;}
        public string PositionIn { get; set; }

        public virtual MsAttendanceMappingAttendance AttendanceMappingAttendance {get;set;}
        public virtual TrGeneratedScheduleLesson GeneratedScheduleLesson { get; set; }
        public virtual ICollection<TrAttendanceEntryWorkhabit> AttendanceEntryWorkhabits { get; set; }

    }

    internal class TrAttendanceEntryDetailConfiguration : AuditEntityConfiguration<TrAttendanceEntry>
    {
        public override void Configure(EntityTypeBuilder<TrAttendanceEntry> builder)
        {
             builder.HasOne(x => x.GeneratedScheduleLesson)
                .WithMany(x => x.AttendanceEntries)
                .HasForeignKey(fk => fk.IdGeneratedScheduleLesson)
                .HasConstraintName("FK_TrAttendanceEntry_TrGenerateScheduleLesson")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.AttendanceMappingAttendance)
                .WithMany(x => x.AttendanceEntries)
                .HasForeignKey(fk => fk.IdAttendanceMappingAttendance)
                .HasConstraintName("FK_TrAttendanceEntry_MsAttendanceMappingAttendance")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Property(x => x.FileEvidence)
                .HasMaxLength(450);

            builder.Property(x => x.Notes)
                .HasMaxLength(450);
            
            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(12)
                .IsRequired();

            builder.HasIndex(p => p.Status);

            builder.Property(x => x.PositionIn)
                .HasMaxLength(5);

            base.Configure(builder);
        }
    }
}
