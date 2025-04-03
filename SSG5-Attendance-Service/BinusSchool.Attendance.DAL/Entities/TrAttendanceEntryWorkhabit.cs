using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class TrAttendanceEntryWorkhabit : AuditEntity, IAttendanceEntity
    {
        public string IdAttendanceEntry {get;set;}
        public string IdMappingAttendanceWorkhabit {get;set;}

        public virtual TrAttendanceEntry AttendanceEntry {get;set;}
        public virtual MsMappingAttendanceWorkhabit MappingAttendanceWorkhabit {get;set;}
        
    }

    internal class TrAttendanceEntryDetailWorkhabitConfiguration : AuditEntityConfiguration<TrAttendanceEntryWorkhabit>
    {
        public override void Configure(EntityTypeBuilder<TrAttendanceEntryWorkhabit> builder)
        {
            builder.HasOne(x => x.AttendanceEntry)
                .WithMany(x => x.AttendanceEntryWorkhabits)
                .HasForeignKey(fk => fk.IdAttendanceEntry)
                .HasConstraintName("FK_TrAttendanceWorkhabit_TrAttendance")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.MappingAttendanceWorkhabit)
                .WithMany(x => x.AttendanceEntryWorkhabits)
                .HasForeignKey(fk => fk.IdMappingAttendanceWorkhabit)
                .HasConstraintName("FK_TrAttendanceWorkhabit_MsMappingAttendanceWorkhabit")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
