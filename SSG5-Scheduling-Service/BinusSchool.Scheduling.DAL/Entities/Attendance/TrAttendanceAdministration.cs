using System;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrAttendanceAdministration :  AuditEntity, ISchedulingEntity
    {
        public string IdStudentGrade {get;set;}
        public string IdAttendance {get;set;}
        public DateTime StartDate {get;set;}
        public DateTime EndDate {get;set;}
        public TimeSpan StartTime {get;set;}
        public TimeSpan EndTime {get;set;}
        public string Reason {get;set;}
        public string AbsencesFile {get;set;}
        public bool IncludeElective {get;set;}
        public bool NeedValidation {get;set;}
        public int SessionUsed { get; set; }

        public virtual MsStudentGrade StudentGrade {get;set;}
        public virtual MsAttendance Attendance {get;set;}
    }

    internal class TrAttendanceAdministrationConfiguration : AuditEntityConfiguration<TrAttendanceAdministration>
    {
        public override void Configure(EntityTypeBuilder<TrAttendanceAdministration> builder)
        {

            builder.Property(x=>x.Reason).HasMaxLength(128);
            builder.Property(x=>x.AbsencesFile).HasMaxLength(450);

            builder.HasOne(x => x.StudentGrade)
                .WithMany(x => x.AttendanceAdministrations)
                .HasForeignKey(fk => fk.IdStudentGrade)
                .HasConstraintName("FK_TrAttendanceAdministration_MsStudentGrade")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.Attendance)
                .WithMany(x => x.AttendanceAdministrations)
                .HasForeignKey(fk => fk.IdAttendance)
                .HasConstraintName("FK_TrAttendanceAdministration_MsAttendance")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
