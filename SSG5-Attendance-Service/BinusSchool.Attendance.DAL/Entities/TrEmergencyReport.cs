using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.AttendanceDb.Entities.School;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class TrEmergencyReport : AuditEntity, IAttendanceEntity
    {
        public string IdAcademicYear { get; set; }
        public string StartedBy { get; set; }
        public DateTime StartedDate { get; set; }
        public bool SubmitStatus { get; set; }
        public string ReportedBy { get; set; }
        public DateTime? ReportedDate { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<TrEmergencyAttendance> EmergencyAttendances { get; set; }
        public virtual ICollection<HTrEmergencyAttendance> HistoryEmergencyAttendances { get; set; }


    }
    internal class TrEmergencyReportConfiguration : AuditEntityConfiguration<TrEmergencyReport>
    {
        public override void Configure(EntityTypeBuilder<TrEmergencyReport> builder)
        {

            builder.Property(x => x.IdAcademicYear)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.StartedBy)
                .HasMaxLength(36);

            builder.Property(x => x.StartedDate)
                .HasColumnType(typeName: "datetime2")
                .IsRequired();
            
            builder.Property(x => x.ReportedBy)
                .HasMaxLength(36);

            builder.Property(x => x.ReportedDate)
                .HasColumnType(typeName: "datetime2");

            builder.HasOne(x => x.AcademicYear)
               .WithMany(x => x.EmergencyReports)
               .HasForeignKey(fk => fk.IdAcademicYear)
               .HasConstraintName("FK_TrEmergencyReport_MsAcademicYear")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
