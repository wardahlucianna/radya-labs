using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class TrUserEventAttendanceWorkhabit2 : AuditEntity, IAttendanceEntity
    {
        public string IdUserEventAttendance { get; set; }
        public string IdMappingAttendanceWorkhabit { get; set; }

        public virtual TrUserEventAttendance2 UserEventAttendance { get; set; }
        public virtual MsMappingAttendanceWorkhabit MappingAttendanceWorkhabit { get; set; }
    }

    internal class TrUserEventAttendanceWorkhabit2Configuration : AuditEntityConfiguration<TrUserEventAttendanceWorkhabit2>
    {
        public override void Configure(EntityTypeBuilder<TrUserEventAttendanceWorkhabit2> builder)
        {
            builder.HasOne(x => x.UserEventAttendance)
                .WithMany(x => x.UserEventAttendanceWorkhabits)
                .HasForeignKey(fk => fk.IdUserEventAttendance)
                .HasConstraintName("FK_TrUserEventAttendanceWorkhabit2_TrUserEventAttendance")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.MappingAttendanceWorkhabit)
                .WithMany(x => x.UserEventAttendanceWorkhabit2s)
                .HasForeignKey(fk => fk.IdMappingAttendanceWorkhabit)
                .HasConstraintName("FK_TrUserEventAttendanceWorkhabit2_MsMappingAttendanceWorkhabit")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
