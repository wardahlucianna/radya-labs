using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class MsAttendance : CodeEntity, IAttendanceEntity
    {
        public string IdAcademicYear {get;set;}
        public AttendanceCategory AttendanceCategory {get;set;}
        public AbsenceCategory? AbsenceCategory {get;set;}
        public ExcusedAbsenceCategory? ExcusedAbsenceCategory {get;set;}
        public AttendanceStatus Status {get;set;}
        public bool IsNeedFileAttachment {get;set;}

        public virtual MsAcademicYear AcademicYear {get;set;}
        public virtual ICollection<MsAttendanceMappingAttendance> AttendanceMappingAttendances {get;set;}
        public virtual ICollection<MsListMappingAttendanceAbsent> ListMappingAttendanceAbsents {get;set;}

        public virtual ICollection<MsMappingAttendanceQuota> MappingAttendanceQuotas {get;set;}

        public virtual ICollection<TrAttendanceAdministration> AttendanceAdministrations {get;set;}

        public virtual ICollection<TrAttendanceSummaryMappingAtd> AttendanceSummaryMappingAtds { get; set; }
    }

    internal class MsAttendanceConfiguration : CodeEntityConfiguration<MsAttendance>
    {
        public override void Configure(EntityTypeBuilder<MsAttendance> builder)
        {
            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.Attendances)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsAttendance_MsAcademicYear")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Property(x => x.AttendanceCategory)
                .HasConversion<string>()
                .HasMaxLength(7)
                .IsRequired();

            builder.Property(x => x.AbsenceCategory)
                .HasConversion<string>()
                .HasMaxLength(9);

            builder.Property(x => x.ExcusedAbsenceCategory)
                .HasConversion<string>()
                .HasMaxLength(14);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(8)
                .IsRequired();

            builder.Property(x => x.IsNeedFileAttachment)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
