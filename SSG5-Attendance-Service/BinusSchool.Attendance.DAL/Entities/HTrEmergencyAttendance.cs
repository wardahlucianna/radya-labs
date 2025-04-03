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
    public class HTrEmergencyAttendance : AuditNoUniqueEntity, IAttendanceEntity
    {
        public string IdEmergencyAttendanceHistory {  get; set; }
        public string IdStudent { get; set; }
        public string IdEmergencyReport { get; set; }
        public string IdEmergencyStatus { get; set; }
        public string Description { get; set; }
        public bool? SendEmailStatus { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual TrEmergencyReport EmergencyReport { get; set; }
        public virtual LtEmergencyStatus EmergencyStatus { get; set; }
    }
    internal class HTrEmergencyAttendanceonfiguration : AuditNoUniqueEntityConfiguration<HTrEmergencyAttendance>
    {
        public override void Configure(EntityTypeBuilder<HTrEmergencyAttendance> builder)
        {
            builder.HasKey(x => x.IdEmergencyAttendanceHistory);

            builder.Property(x => x.IdEmergencyAttendanceHistory)
                .HasMaxLength(36)
                .IsRequired();

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
                .WithMany(x => x.HistoryEmergencyAttendances)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_HTrEmergencyAttendance_MsStudent")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.EmergencyReport)
                .WithMany(x => x.HistoryEmergencyAttendances)
                .HasForeignKey(fk => fk.IdEmergencyReport)
                .HasConstraintName("FK_HTrEmergencyAttendance_TrEmergencyReport")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.EmergencyStatus)
                .WithMany(x => x.HistoryEmergencyAttendances)
                .HasForeignKey(fk => fk.IdEmergencyStatus)
                .HasConstraintName("FK_HTrEmergencyAttendance_LtEmergencyStatus")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);

        }
    }
}
