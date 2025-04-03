using System;
using System.Collections.Generic;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class TrAttdSummaryLogSch : AuditEntity, IAttendanceEntity
    {
        public TrAttdSummaryLogSch()
        {
            Id = Guid.NewGuid().ToString();
        }
        
        public string IdAttendanceSummaryLog { get; set; }
        public string IdSchool { get; set; }
        public string SchoolName { get; set; }
        public int TotalGrade { get; set; }
        public int TotalStudent { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual TrAttendanceSummaryLog AttendanceSummaryLog { get; set; }
        public virtual ICollection<TrAttdSummaryLogSchGrd> AttdSummaryLogSchGrd { get; set; }
    }

    internal class TrAttdSummaryLogSchConfiguration : AuditEntityConfiguration<TrAttdSummaryLogSch>
    {
        public override void Configure(EntityTypeBuilder<TrAttdSummaryLogSch> builder)
        {
            builder.HasOne(x => x.School)
               .WithMany(x => x.AttdSummaryLogSch)
               .HasForeignKey(fk => fk.IdSchool)
               .HasConstraintName("FK_TrAttdSummaryLogSch_MsSchool")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            builder.HasOne(x => x.AttendanceSummaryLog)
               .WithMany(x => x.AttdSummaryLogSch)
               .HasForeignKey(fk => fk.IdAttendanceSummaryLog)
               .HasConstraintName("FK_TrAttdSummaryLogSch_TrAttendanceSummaryLog")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

            builder.Property(x => x.SchoolName)
                .HasMaxLength(50)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
