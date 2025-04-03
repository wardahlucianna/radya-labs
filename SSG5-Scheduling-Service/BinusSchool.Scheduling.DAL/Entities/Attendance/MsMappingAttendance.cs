using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsMappingAttendance :  AuditEntity, ISchedulingEntity
    {
        public string IdLevel {get;set;}
        public AbsentTerm AbsentTerms {get;set;}
        public bool IsNeedValidation {get;set;}
        public bool IsUseWorkhabit {get;set;}
        public bool IsUseDueToLateness { get; set; }
        public bool UsingCheckboxAttendance { get; set; }
        public bool ShowingModalReminderAttendanceEntry { get; set; }
        public RenderAttendance RenderAttendance { get; set; }

        public virtual MsLevel Level {get;set;}
        public virtual ICollection<MsMappingAttendanceWorkhabit> MappingAttendanceWorkhabits { get; set; }
        public virtual ICollection<MsAbsentMappingAttendance> AbsentMappingAttendances { get; set; }
        public virtual ICollection<MsAttendanceMappingAttendance> AttendanceMappingAttendances { get; set; }
    }

    internal class MsMappingAttendanceConfiguration : AuditEntityConfiguration<MsMappingAttendance>
    {
        public override void Configure(EntityTypeBuilder<MsMappingAttendance> builder)
        {

            builder.Property(x => x.AbsentTerms)
                .HasConversion<string>()
                .HasMaxLength(7)
                .IsRequired();

            builder.Property(x => x.RenderAttendance)
                .HasConversion<string>()
                .HasMaxLength(12)
                .IsRequired();

            builder.Property(x => x.IsNeedValidation)
                .IsRequired();

            builder.Property(x => x.IsUseWorkhabit)
                .IsRequired();

            builder.HasOne(x => x.Level)
                .WithMany(x => x.MappingAttendances)
                .HasForeignKey(fk => fk.IdLevel)
                .HasConstraintName("FK_MsMappingAttendances_MsLevel")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
