using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class TrUserEventAttendance2 : AuditEntity, IAttendanceEntity
    {
        public string IdUserEvent { get; set; }
        public string IdEventIntendedForAtdCheckStudent { get; set; }
        public string IdAttendanceMappingAttendance { get; set; }
        public TimeSpan? LateTime { get; set; }
        public string FileEvidence { get; set; }
        public string Notes { get; set; }
        public DateTime DateEvent { get; set; }
        public bool HasBeenResolved { get; set; }
        public bool HasBeenChoose { get; set; }

        public virtual TrUserEvent UserEvent { get; set; }
        public virtual TrEventIntendedForAtdCheckStudent EventIntendedForAttendanceCheckStudent { get; set; }
        public virtual MsAttendanceMappingAttendance AttendanceMappingAttendance { get; set; }
        public virtual ICollection<TrUserEventAttendanceWorkhabit2> UserEventAttendanceWorkhabits { get; set; }
    }

    internal class TrUserEventAttendance2Configuration : AuditEntityConfiguration<TrUserEventAttendance2>
    {
        public override void Configure(EntityTypeBuilder<TrUserEventAttendance2> builder)
        {
            builder.HasOne(x => x.UserEvent)
               .WithMany(x => x.UserEventAttendance2s)
               .HasForeignKey(fk => fk.IdUserEvent)
               .HasConstraintName("FK_TrUserEventAttendance2_MsUserEvent")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            builder.HasOne(x => x.EventIntendedForAttendanceCheckStudent)
               .WithMany(x => x.UserEventAttendance2s)
               .HasForeignKey(fk => fk.IdEventIntendedForAtdCheckStudent)
               .HasConstraintName("FK_TrUserEventAttendance2_MsEventIntendedForAttendanceCheckStudent")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            builder.HasOne(x => x.AttendanceMappingAttendance)
               .WithMany(x => x.UserEventAttendance2s)
               .HasForeignKey(fk => fk.IdAttendanceMappingAttendance)
               .HasConstraintName("FK_TrUserEventAttendance2_MsAttendanceMappingAttendance")
               .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);

        }
    }
}
