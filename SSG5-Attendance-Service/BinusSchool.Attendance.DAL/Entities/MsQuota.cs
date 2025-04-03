using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class MsQuota : AuditEntity, IAttendanceEntity
    {
        public string IdLevel {get;set;}
        public string IdAcademicYear {get;set; }
        public virtual MsLevel Level {get;set;}
        public virtual MsAcademicYear AcademicYear {get;set; }
        public virtual ICollection<MsQuotaDetail> QuotaDetails { get; set; }
    }

    internal class MsQuotaConfiguration : AuditEntityConfiguration<MsQuota>
    {
        public override void Configure(EntityTypeBuilder<MsQuota> builder)
        {
            builder.HasOne(x => x.Level)
               .WithMany(x => x.Quota)
               .HasForeignKey(fk => fk.IdLevel)
               .HasConstraintName("FK_MsQuota_MsLevel")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.Quota)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsQuota_MsAcademicYear")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
