using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class TrUserEventAttendanceWorkhabit : AuditEntity, IAttendanceEntity
    {
        public string IdUserEventAttendance { get; set; }
        public string IdMappingAttendanceWorkhabit { get; set; }

        public virtual TrUserEventAttendance UserEventAttendance { get; set; }
        public virtual MsMappingAttendanceWorkhabit MappingAttendanceWorkhabit { get; set; }
    }

    internal class TrUserEventAttendanceWorkhabitConfiguration : AuditEntityConfiguration<TrUserEventAttendanceWorkhabit>
    {
        public override void Configure(EntityTypeBuilder<TrUserEventAttendanceWorkhabit> builder)
        {
            builder.HasOne(x => x.UserEventAttendance)
                .WithMany(x => x.UserEventAttendanceWorkhabits)
                .HasForeignKey(fk => fk.IdUserEventAttendance)
                .HasConstraintName("FK_TrUserEventAttendanceWorkhabit_TrUserEventAttendance")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.MappingAttendanceWorkhabit)
                .WithMany(x => x.UserEventAttendanceWorkhabits)
                .HasForeignKey(fk => fk.IdMappingAttendanceWorkhabit)
                .HasConstraintName("FK_TrUserEventAttendanceWorkhabit_MsMappingAttendanceWorkhabit")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
