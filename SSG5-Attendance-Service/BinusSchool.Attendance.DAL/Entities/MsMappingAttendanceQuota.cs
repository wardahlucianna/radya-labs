using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class MsMappingAttendanceQuota : AuditEntity, IAttendanceEntity
    {
        public string IdLevel {get;set;}
        public string IdAttendance {get;set;}
        public decimal Percentage {get;set;}
        
        public virtual MsAttendance Attendance {get;set;}
        public virtual MsLevel Level {get;set;}
    }

    internal class MsMappingAttendanceQuotaConfiguration : AuditEntityConfiguration<MsMappingAttendanceQuota>
    {
        public override void Configure(EntityTypeBuilder<MsMappingAttendanceQuota> builder)
        {
            builder.HasOne(x => x.Level)
                .WithMany(x => x.MappingAttendanceQuotas)
                .HasForeignKey(fk => fk.IdLevel)
                .HasConstraintName("FK_MsMappingAttendanceQuota_MsLevel")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Attendance)
                .WithMany(x => x.MappingAttendanceQuotas)
                .HasForeignKey(fk => fk.IdAttendance)
                .HasConstraintName("FK_MsMappingAttendanceQuota_MsAttendance")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.Property(x=>x.Percentage)
                .HasColumnType("decimal(5,2)");

            base.Configure(builder);
        }
    }
}
