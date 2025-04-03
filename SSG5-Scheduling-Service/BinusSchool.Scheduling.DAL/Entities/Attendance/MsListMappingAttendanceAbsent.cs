using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsListMappingAttendanceAbsent : AuditEntity, ISchedulingEntity
    {
        public string IdAbsentMappingAttendance {get;set;}
        public string IdAttendance {get;set;}
        public bool IsNeedValidation { get; set; }

        public virtual MsAttendance MsAttendance {get;set;}
        public virtual MsAbsentMappingAttendance MsAbsentMappingAttendance {get;set;}
    }

    internal class MsListMappingAttendanceAbsentConfiguration : AuditEntityConfiguration<MsListMappingAttendanceAbsent>
    {
        public override void Configure(EntityTypeBuilder<MsListMappingAttendanceAbsent> builder)
        {
            builder.Property(x => x.IsNeedValidation)
                .IsRequired();
                
            builder.HasOne(x => x.MsAttendance)
                .WithMany(x => x.ListMappingAttendanceAbsents)
                .HasForeignKey(fk => fk.IdAttendance)
                .HasConstraintName("FK_MsListMappingAttendanceAbsent_MsAttendance")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.MsAbsentMappingAttendance)
                .WithMany(x => x.ListMappingAttendanceAbsents)
                .HasForeignKey(fk => fk.IdAbsentMappingAttendance)
                .HasConstraintName("Fk_MsListMappingAttendanceAbsent_MsAbsentMappingAttendance")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
