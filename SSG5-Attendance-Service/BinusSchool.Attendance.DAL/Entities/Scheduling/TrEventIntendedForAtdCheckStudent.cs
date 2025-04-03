using System;
using System.Collections.Generic;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Scheduling
{
    public class TrEventIntendedForAtdCheckStudent : AuditEntity, IAttendanceEntity
    {
        public string IdEventIntendedForAttendanceStudent { get; set; }
        public string CheckName { get; set; }
        public TimeSpan Time { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime StartDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public virtual TrEventIntendedForAttendanceStudent EventIntendedForAttendanceStudent { get; set; }
        public virtual ICollection<TrUserEventAttendance2> UserEventAttendance2s { get; set; }
        public virtual ICollection<TrUserEventAttendance> UserEventAttendances { get; set; }

    }

    internal class TrEventIntendedForAtdCheckStudentConfiguration : AuditEntityConfiguration<TrEventIntendedForAtdCheckStudent>
    {
        public override void Configure(EntityTypeBuilder<TrEventIntendedForAtdCheckStudent> builder)
        {
            builder.Property(x => x.CheckName).IsRequired().HasMaxLength(50);

            builder.HasOne(x => x.EventIntendedForAttendanceStudent)
           .WithMany(x => x.EventIntendedForAtdCheckStudents)
           .HasForeignKey(fk => fk.IdEventIntendedForAttendanceStudent)
           .HasConstraintName("FK_TrEventIntendedForAtdCheckStudent_TrEventIntendedForAttendanceStudent")
           .OnDelete(DeleteBehavior.Restrict)
           .IsRequired();

            base.Configure(builder);
        }
    }
}
