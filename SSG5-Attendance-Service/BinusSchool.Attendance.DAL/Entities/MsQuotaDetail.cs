using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class MsQuotaDetail : AuditEntity, IAttendanceEntity
    {
        public string IdQuota {get;set;}
        public AttendanceCategory AttendanceCategory {get;set;}
        public AbsenceCategory? AbsenceCategory {get;set;}
        public ExcusedAbsenceCategory? ExcusedAbsenceCategory {get;set;}
        public AttendanceStatus Status {get;set;}
        public decimal Percentage { get; set; }
        public virtual MsQuota Quota {get;set;}
    }

    internal class MsQuotaDetailConfiguration : AuditEntityConfiguration<MsQuotaDetail>
    {
        public override void Configure(EntityTypeBuilder<MsQuotaDetail> builder)
        {
            builder.HasOne(x => x.Quota)
               .WithMany(x => x.QuotaDetails)
               .HasForeignKey(fk => fk.IdQuota)
               .HasConstraintName("FK_MsQuotaDetail_MsQuota")
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

            builder.Property(x => x.Percentage)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
