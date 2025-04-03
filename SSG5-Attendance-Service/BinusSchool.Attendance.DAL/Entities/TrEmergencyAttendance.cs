using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class TrEmergencyAttendance : AuditEntity, IAttendanceEntity
    {
        public string IdStudent { get; set; }
        public string IdEmergencyReport { get; set; }
        public string IdEmergencyStatus { get; set; }
        public string Description { get; set; }
        public bool? SendEmailStatus { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual TrEmergencyReport EmergencyReport { get; set; }
        public virtual LtEmergencyStatus EmergencyStatus { get; set; }
    }
    internal class TrEmergencyAttendanceConfiguration : AuditEntityConfiguration<TrEmergencyAttendance>
    {
        public override void Configure(EntityTypeBuilder<TrEmergencyAttendance> builder)
        {

            builder.Property(x => x.IdStudent)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdEmergencyReport)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdEmergencyStatus)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(100);

            builder.HasOne(x => x.Student)
                 .WithMany(x => x.EmergencyAttendances)
                 .HasForeignKey(fk => fk.IdStudent)
                 .HasConstraintName("FK_TrEmergencyAttendance_MsStudent")
                 .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.EmergencyReport)
                .WithMany(x => x.EmergencyAttendances)
                .HasForeignKey(fk => fk.IdEmergencyReport)
                .HasConstraintName("FK_TrEmergencyAttendance_TrEmergencyReport")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.EmergencyStatus)
                .WithMany(x => x.EmergencyAttendances)
                .HasForeignKey(fk => fk.IdEmergencyStatus)
                .HasConstraintName("FK_TrEmergencyAttendance_LtEmergencyStatus")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
