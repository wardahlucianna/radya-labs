using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Scheduling
{
    public class TrEventIntendedForAttendanceStudent : AuditEntity, IAttendanceEntity
    {
        public string IdEventIntendedFor { get; set; }
        public EventIntendedForAttendanceStudent Type { get; set; }
        public bool IsSetAttendance { get; set; }
        public bool IsRepeat { get; set; }
        public virtual TrEventIntendedFor EventIntendedFor { get; set; }
        public virtual ICollection<TrEventIntendedForAtdPICStudent> EventIntendedForAtdPICStudents { get; set; }
        public virtual ICollection<TrEventIntendedForAtdCheckStudent> EventIntendedForAtdCheckStudents { get; set; }
    }
    internal class TrEventIntendedForAttendanceStudentConfiguration : AuditEntityConfiguration<TrEventIntendedForAttendanceStudent>
    {
        public override void Configure(EntityTypeBuilder<TrEventIntendedForAttendanceStudent> builder)
        {
            builder.Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(10)
                .IsRequired();

            builder.HasOne(x => x.EventIntendedFor)
             .WithMany(x => x.EventIntendedForAttendanceStudents)
             .HasForeignKey(fk => fk.IdEventIntendedFor)
             .HasConstraintName("FK_TrEventIntendedForAttendanceStudent_TrEventIntendedFor")
             .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            base.Configure(builder);
        }
    }
}
