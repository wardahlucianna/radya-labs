using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Employee;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class TrAttendanceEntryV2 : AuditNoUniqueEntity, IAttendanceEntity
    {
        public string IdAttendanceEntry { get; set; }
        public string IdScheduleLesson { get; set; }
        public string IdAttendanceMappingAttendance { get; set; }
        public string IdBinusian { get; set; }
        public string IdHomeroomStudent { get; set; }
        public TimeSpan? LateTime { get; set; }
        public string FileEvidence { get; set; }
        public string Notes { get; set; }
        public AttendanceEntryStatus Status { get; set; }
        public bool IsFromAttendanceAdministration { get; set; }
        public string PositionIn { get; set; }

        public virtual MsScheduleLesson ScheduleLesson { get; set; }
        public virtual MsAttendanceMappingAttendance AttendanceMappingAttendance { get; set; }
        public virtual MsHomeroomStudent HomeroomStudent { get; set; }
        public virtual MsStaff Staff { get; set; }
        public virtual ICollection<TrAttendanceEntryWorkhabitV2> AttendanceEntryWorkhabitV2s { get; set; }
    }

    internal class TrAttendanceEntryV2Configuration : AuditNoUniqueEntityConfiguration<TrAttendanceEntryV2>
    {
        public override void Configure(EntityTypeBuilder<TrAttendanceEntryV2> builder)
        {
            builder.HasKey(x => x.IdAttendanceEntry);
            builder.Property(p => p.IdAttendanceEntry).HasMaxLength(36).IsRequired();

            builder.HasOne(x => x.ScheduleLesson)
               .WithMany(x => x.AttendanceEntryV2s)
               .HasForeignKey(fk => fk.IdScheduleLesson)
               .HasConstraintName("FK_TrAttendanceEntryV2_MsScheduleLesson")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

            builder.HasOne(x => x.AttendanceMappingAttendance)
                .WithMany(x => x.AttendanceEntryV2s)
                .HasForeignKey(fk => fk.IdAttendanceMappingAttendance)
                .HasConstraintName("FK_TrAttendanceEntryV2_MsAttendanceMappingAttendance")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.HomeroomStudent)
                .WithMany(x => x.AttendanceEntryV2s)
                .HasForeignKey(fk => fk.IdHomeroomStudent)
                .HasConstraintName("FK_TrAttendanceEntryV2_MsHomeroomStudent")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Staff)
                .WithMany(x => x.AttendanceEntryV2s)
                .HasForeignKey(fk => fk.IdBinusian)
                .HasConstraintName("FK_TrAttendanceEntryV2_MsStaff")
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
                .HasMaxLength(10);

            base.Configure(builder);
        }
    }
}
