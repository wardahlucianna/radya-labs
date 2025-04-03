using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class TrAttdAdministrationCancel : AuditEntity, IAttendanceEntity
    {
        public string IdAttendanceAdministration { get; set; }
        public string IdScheduleLesson { get; set; }
        public virtual TrAttendanceAdministration AttendanceAdministration { get; set; }
        public virtual MsScheduleLesson ScheduleLesson { get; set; }
    }

    internal class TrAttdAdministrationCancelConfiguration : AuditEntityConfiguration<TrAttdAdministrationCancel>
    {
        public override void Configure(EntityTypeBuilder<TrAttdAdministrationCancel> builder)
        {
            builder.HasOne(x => x.AttendanceAdministration)
                .WithMany(x => x.AttdAdministrationCancel)
                .HasForeignKey(fk => fk.IdAttendanceAdministration)
                .HasConstraintName("FK_TrAttdAdministrationCancel_TrAttendanceAdministration")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.ScheduleLesson)
                .WithMany(x => x.AttdAdministrationCancel)
                .HasForeignKey(fk => fk.IdScheduleLesson)
                .HasConstraintName("FK_TrAttdAdministrationCancel_MsScheduleLesson")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
