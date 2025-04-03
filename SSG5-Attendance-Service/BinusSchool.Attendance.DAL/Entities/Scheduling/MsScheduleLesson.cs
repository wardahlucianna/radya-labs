using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Scheduling
{
    public class MsScheduleLesson : AuditEntity, IAttendanceEntity
    {
        public string IdWeek { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string IdVenue { get; set; }
        public string VenueName { get; set; }
        public string ClassID { get; set; }
        public string IdLesson { get; set; }
        public bool IsGenerated { get; set; }
        public string DaysOfWeek { get; set; }
        public TimeSpan EndTime { get; set; }
        public string IdSession { get; set; }
        public string IdSubject { get; set; }
        public string SessionID { get; set; }
        public TimeSpan StartTime { get; set; }
        public string SubjectName { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdDay { get; set; }
        public bool IsDeleteFromEvent { get; set; }
        public virtual MsWeek Week { get; set; }
        public virtual MsVenue Venue { get; set; }
        public virtual MsLesson Lesson { get; set; }
        public virtual MsSession Session { get; set; }
        public virtual MsSubject Subject { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsLevel Level { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual LtDay Day { get; set; }
        public virtual ICollection<TrAttendanceEntryV2> AttendanceEntryV2s { get; set; }
        public virtual ICollection<TrAttdAdministrationCancel> AttdAdministrationCancel { get; set; }
    }

    internal class MsScheduleLessonConfiguration : AuditEntityConfiguration<MsScheduleLesson>
    {
        public override void Configure(EntityTypeBuilder<MsScheduleLesson> builder)
        {
            builder.Property(x => x.IdWeek)
              .HasMaxLength(36)
              .IsRequired();

            builder.Property(x => x.ScheduleDate)
              .IsRequired();

            builder.Property(x => x.IdWeek)
             .HasMaxLength(36)
             .IsRequired();


            builder.Property(x => x.IdVenue)
            .HasMaxLength(36)
            .IsRequired();

            builder.Property(x => x.ClassID)
             .HasMaxLength(36)
            .IsRequired();

            builder.Property(x => x.IdLesson)
            .HasMaxLength(36);

            builder.Property(x => x.IdSubject)
           .HasMaxLength(36);

            builder.Property(x => x.SubjectName)
            .HasMaxLength(128);

            builder.Property(x => x.IdSession)
           .HasMaxLength(36);

            builder.Property(x => x.SessionID)
            .HasMaxLength(36);

            builder.Property(x => x.DaysOfWeek)
           .HasMaxLength(36);

            builder.Property(x => x.VenueName)
            .HasMaxLength(128)
           .IsRequired();

            builder.HasOne(x => x.Subject)
                .WithMany(x => x.ScheduleLesson)
                .HasForeignKey(fk => fk.IdSubject)
                .HasConstraintName("FK_MsScheduleLesson_MsSubject")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            builder.HasOne(x => x.Week)
              .WithMany(x => x.ScheduleLesson)
              .HasForeignKey(fk => fk.IdWeek)
              .HasConstraintName("FK_MsScheduleLesson_MsWeek")
              .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Venue)
            .WithMany(x => x.ScheduleLesson)
            .HasForeignKey(fk => fk.IdVenue)
            .HasConstraintName("FK_MsScheduleLesson_MsVenue")
            .OnDelete(DeleteBehavior.Cascade)
              .IsRequired();

            builder.HasOne(x => x.Lesson)
            .WithMany(x => x.ScheduleLesson)
            .HasForeignKey(fk => fk.IdLesson)
            .HasConstraintName("FK_MsScheduleLesson_MsLesson")
            .OnDelete(DeleteBehavior.Cascade)
              .IsRequired();

            builder.HasOne(x => x.Session)
            .WithMany(x => x.ScheduleLesson)
            .HasForeignKey(fk => fk.IdSession)
            .HasConstraintName("FK_MsScheduleLesson_MsSession")
            .OnDelete(DeleteBehavior.Cascade)
              .IsRequired();

            builder.HasOne(x => x.AcademicYear)
           .WithMany(x => x.ScheduleLesson)
           .HasForeignKey(fk => fk.IdAcademicYear)
           .HasConstraintName("FK_MsScheduleLesson_MsAcademicYear")
           .OnDelete(DeleteBehavior.Cascade)
           .IsRequired()
           ;

            builder.HasOne(x => x.Level)
           .WithMany(x => x.ScheduleLesson)
           .HasForeignKey(fk => fk.IdLevel)
           .HasConstraintName("FK_MsScheduleLesson_MsLevel")
           .OnDelete(DeleteBehavior.NoAction)
           .IsRequired()
           ;

            builder.HasOne(x => x.Grade)
           .WithMany(x => x.ScheduleLesson)
           .HasForeignKey(fk => fk.IdGrade)
           .HasConstraintName("FK_MsScheduleLesson_MsGrade")
           .OnDelete(DeleteBehavior.NoAction)
           .IsRequired()
           ;

            builder.HasOne(x => x.Day)
           .WithMany(x => x.ScheduleLesson)
           .HasForeignKey(fk => fk.IdDay)
           .HasConstraintName("FK_MsScheduleLesson_LtDay")
           .OnDelete(DeleteBehavior.NoAction)
           .IsRequired()
           ;

            base.Configure(builder);
        }
    }
}
