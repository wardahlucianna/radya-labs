using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class TrAttendanceSummaryWorkhabit : AuditEntity, IAttendanceEntity
    {
        public string IdAttendanceSummary { get; set; }
        public string IdWorkHabit { get; set; }
        public int CountAsDay { get; set; }
        public int CountAsSession { get; set; }

        public virtual TrAttendanceSummary AttendanceSummary { get; set; }
        public virtual MsWorkhabit Workhabit { get; set; }

    }
    internal class TrAttendanceSummaryWorkhabitConfiguration : AuditEntityConfiguration<TrAttendanceSummaryWorkhabit>
    {
        public override void Configure(EntityTypeBuilder<TrAttendanceSummaryWorkhabit> builder)
        {
            builder.HasOne(x => x.AttendanceSummary)
               .WithMany(x => x.AttendanceSummaryWorkhabits)
               .HasForeignKey(fk => fk.IdAttendanceSummary)
               .HasConstraintName("FK_TrAttendanceSummaryWorkhabit_TrAttendanceSummary")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

            builder.HasOne(x => x.Workhabit)
               .WithMany(x => x.AttendanceSummaryWorkhabits)
               .HasForeignKey(fk => fk.IdWorkHabit)
               .HasConstraintName("FK_TrAttendanceSummaryWorkhabit_MsWorkhabit")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
