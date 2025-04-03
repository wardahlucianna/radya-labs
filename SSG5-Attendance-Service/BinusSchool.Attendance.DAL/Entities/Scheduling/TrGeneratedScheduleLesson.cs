using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Scheduling
{
    public class TrGeneratedScheduleLesson : AuditEntity, IAttendanceEntity
    {
        public string IdGeneratedScheduleStudent { get; set; }
        public string IdWeek { get; set; }
        public DateTime StartPeriod { get; set; }
        public DateTime EndPeriod { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string IdUser { get; set; }
        public string TeacherName { get; set; }
        public string IdVenue { get; set; }
        public string VenueName { get; set; }
        public string ClassID { get; set; }
        public string IdLesson { get; set; }
        public string IdSubject { get; set; }
        public string SubjectName { get; set; }
        public string IdHomeroom { get; set; }
        public string HomeroomName { get; set; }
        public string IdSession { get; set; }
        public string SessionID { get; set; }
        public string DaysOfWeek { get; set; }
        public string IdStudent { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsGenerated { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public virtual MsSubject Subject { get; set; }
        public virtual MsWeek Week { get; set; }
        public virtual MsHomeroom Homeroom { get; set; }
        public virtual TrGeneratedScheduleStudent GeneratedScheduleStudent { get; set; }
        public virtual MsSession Session { get; set; }
        public virtual MsVenue Venue { get; set; }
        public virtual MsLesson Lesson { get; set; }
        public virtual MsUser User { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsLevel Level { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual ICollection<TrAttendanceEntry> AttendanceEntries { get; set; }
        public virtual ICollection<TrSendEmailAttendanceEntry> SendEmailAttendanceEntries { get; set; }
    }

    internal class TrGeneratedScheduleLessonConfiguration : AuditEntityConfiguration<TrGeneratedScheduleLesson>
    {
        public override void Configure(EntityTypeBuilder<TrGeneratedScheduleLesson> builder)
        {
            builder.Property(x => x.IdGeneratedScheduleStudent)
                           .HasMaxLength(36)
                           .IsRequired();

            builder.Property(x => x.IdWeek)
              .HasMaxLength(36)
              .IsRequired();

            builder.Property(x => x.StartPeriod)
              .IsRequired();

            builder.Property(x => x.EndPeriod)
                .IsRequired();

            builder.Property(x => x.ScheduleDate)
              .IsRequired();

            builder.Property(x => x.IdWeek)
             .HasMaxLength(36)
             .IsRequired();


            builder.Property(x => x.IdVenue)
            .HasMaxLength(36)
            .IsRequired();

            builder.Property(x => x.IdUser)
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

            builder.Property(x => x.IdHomeroom)
           .HasMaxLength(36);

            builder.Property(x => x.HomeroomName)
            .HasMaxLength(450);

            builder.Property(x => x.IdSession)
           .HasMaxLength(36);

            builder.Property(x => x.SessionID)
            .HasMaxLength(36);

            builder.Property(x => x.DaysOfWeek)
           .HasMaxLength(36);

            builder.Property(x => x.VenueName)
            .HasMaxLength(128)
            .IsRequired();

            builder.Property(x => x.TeacherName)
           .HasMaxLength(100)
            .IsRequired();

            builder.HasOne(x => x.Week)
                .WithMany(x => x.GeneratedScheduleLessons)
                .HasForeignKey(fk => fk.IdWeek)
                .HasConstraintName("FK_TrGeneratedScheduleLesson_MsWeek")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            builder.HasOne(x => x.Subject)
                .WithMany(x => x.GeneratedScheduleLessons)
                .HasForeignKey(fk => fk.IdSubject)
                .HasConstraintName("FK_TrGeneratedScheduleLesson_MsSubject")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Homeroom)
                .WithMany(x => x.GeneratedScheduleLessons)
                .HasForeignKey(fk => fk.IdHomeroom)
                .HasConstraintName("FK_TrGeneratedScheduleLesson_MsHomeroom")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.GeneratedScheduleStudent)
                .WithMany(x => x.GenerateScheduleLessons)
                .HasForeignKey(fk => fk.IdGeneratedScheduleStudent)
                .HasConstraintName("FK_TrGeneratedScheduleLesson_TrGenerateScheduleStudent")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Session)
              .WithMany(x => x.GeneratedScheduleLessons)
              .HasForeignKey(fk => fk.IdSession)
              .HasConstraintName("FK_TrGeneratedScheduleLesson_MsSession")
              .OnDelete(DeleteBehavior.Cascade)
              .IsRequired();


            builder.HasOne(x => x.Venue)
           .WithMany(x => x.GeneratedScheduleLessons)
           .HasForeignKey(fk => fk.IdVenue)
           .HasConstraintName("FK_TrGeneratedScheduleLesson_MsVenue")
           .OnDelete(DeleteBehavior.Cascade)
             .IsRequired();

            builder.HasOne(x => x.Lesson)
            .WithMany(x => x.GeneratedScheduleLessons)
            .HasForeignKey(fk => fk.IdLesson)
            .HasConstraintName("FK_TrGeneratedScheduleLesson_MsLesson")
            .OnDelete(DeleteBehavior.Cascade)
              .IsRequired();

            builder.HasOne(x => x.User)
            .WithMany(x => x.GeneratedScheduleLessons)
            .HasForeignKey(fk => fk.IdUser)
            .HasConstraintName("FK_TrGeneratedScheduleLesson_MsUser")
            .OnDelete(DeleteBehavior.Cascade)
              .IsRequired();

            builder.HasOne(x => x.Student)
            .WithMany(x => x.GeneratedScheduleLessons)
            .HasForeignKey(fk => fk.IdStudent)
            .HasConstraintName("FK_TrGeneratedScheduleLesson_MsStudent")
            .OnDelete(DeleteBehavior.NoAction)
            //.IsRequired()
            ;

            builder.HasOne(x => x.AcademicYear)
           .WithMany(x => x.GeneratedScheduleLessons)
           .HasForeignKey(fk => fk.IdAcademicYear)
           .HasConstraintName("FK_TrGeneratedScheduleLesson_MsAcademicYear")
           .OnDelete(DeleteBehavior.NoAction)
           //.IsRequired()
           ;

            builder.HasOne(x => x.Level)
           .WithMany(x => x.GeneratedScheduleLessons)
           .HasForeignKey(fk => fk.IdLevel)
           .HasConstraintName("FK_TrGeneratedScheduleLesson_MsLevel")
           .OnDelete(DeleteBehavior.NoAction)
           //.IsRequired()
           ;

            builder.HasOne(x => x.Grade)
           .WithMany(x => x.GeneratedScheduleLessons)
           .HasForeignKey(fk => fk.IdGrade)
           .HasConstraintName("FK_TrGeneratedScheduleLesson_MsGrade")
           .OnDelete(DeleteBehavior.NoAction)
           //.IsRequired()
           ;

            base.Configure(builder);
        }
    }
}
