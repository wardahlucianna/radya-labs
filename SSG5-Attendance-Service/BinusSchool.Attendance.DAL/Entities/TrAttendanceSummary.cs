using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class TrAttendanceSummary : AuditEntity, IAttendanceEntity
    {
        public string IdStudent { get; set; }
        public DateTime Date { get; set; }
        public int TotalDays { get; set; }
        public int TotalSession { get; set; }

        public virtual MsStudent Student { get; set; }
        public virtual ICollection<TrAttendanceSummaryMappingAtd> AttendanceSummaryMappingAtds { get; set; }
        public virtual ICollection<TrAttendanceSummaryWorkhabit> AttendanceSummaryWorkhabits { get; set; }
    }

    internal class TrAttendanceSummaryConfiguration : AuditEntityConfiguration<TrAttendanceSummary>
    {
        public override void Configure(EntityTypeBuilder<TrAttendanceSummary> builder)
        {
            builder.HasOne(x => x.Student)
               .WithMany(x => x.AttendanceSummaries)
               .HasForeignKey(fk => fk.IdStudent)
               .HasConstraintName("FK_TrAttendanceSummary_MsStudent")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();          

            base.Configure(builder);
        }
    }
}
