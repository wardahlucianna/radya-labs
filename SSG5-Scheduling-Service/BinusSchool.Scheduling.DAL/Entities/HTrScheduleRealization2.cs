using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrScheduleRealization2 : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string IdHTrScheduleRealization2 { get; set; }
        public string IdScheduleRealization2 { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string IdBinusian { get; set; }
        public string TeacherName { get; set; }
        public string IdBinusianSubtitute { get; set; }
        public string TeacherNameSubtitute { get; set; }
        public string IdVenue { get; set; }
        public string VenueName { get; set; }
        public string IdVenueChange { get; set; }
        public string VenueNameChange { get; set; }
        public string ClassID { get; set; }
        public string SessionID { get; set; }
        public string DaysOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsCancel { get; set; }
        public bool IsSendEmail { get; set; }
        public string NotesForSubtitutions { get; set; }
        public string Status { get; set; }
        public string IdLesson { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdDay { get; set; }
        public virtual TrScheduleRealization2 ScheduleRealization2 { get; set; }
        public virtual MsVenue Venue { get; set; }
        public virtual MsVenue VenueChange { get; set; }
        public virtual MsStaff Staff { get; set; }
        public virtual MsStaff StaffSubtitute { get; set; }
        public virtual MsLesson Lesson { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsLevel Level { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual LtDay Day { get; set; }
    }

    internal class HTrScheduleRealization2Configuration : AuditNoUniqueEntityConfiguration<HTrScheduleRealization2>
    {
        public override void Configure(EntityTypeBuilder<HTrScheduleRealization2> builder)
        {
            builder.HasKey(x => x.IdHTrScheduleRealization2);
            builder.Property(p => p.IdHTrScheduleRealization2).HasMaxLength(36).IsRequired();
            builder.Property(x => x.ScheduleDate)
              .IsRequired();

            builder.Property(x => x.IdBinusian)
               .HasMaxLength(36)
              .IsRequired();

            builder.Property(x => x.IdBinusianSubtitute)
               .HasMaxLength(36)
              .IsRequired();

            builder.Property(x => x.IdVenue)
            .HasMaxLength(36)
            .IsRequired();

            builder.Property(x => x.IdVenueChange)
            .HasMaxLength(36)
            .IsRequired();

            builder.Property(x => x.ClassID)
             .HasMaxLength(36)
            .IsRequired();

            builder.Property(x => x.SessionID)
            .HasMaxLength(36)
            .IsRequired();

            builder.Property(x => x.DaysOfWeek)
           .HasMaxLength(36)
           .IsRequired();

            builder.Property(x => x.VenueName)
            .HasMaxLength(128)
           .IsRequired();

            builder.Property(x => x.VenueNameChange)
            .HasMaxLength(128)
           .IsRequired();

            builder.Property(x => x.TeacherNameSubtitute)
             .HasMaxLength(100)
             .IsRequired();

            builder.Property(x => x.NotesForSubtitutions)
            .HasMaxLength(150);

            builder.Property(x => x.Status)
            .HasMaxLength(50)
            .IsRequired();

            builder.Property(x => x.IdLesson)
           .HasMaxLength(36)
           .IsRequired();

            builder.Property(x => x.IdAcademicYear)
           .HasMaxLength(36)
           .IsRequired();

            builder.Property(x => x.IdLevel)
            .HasMaxLength(36)
            .IsRequired();

            builder.Property(x => x.IdGrade)
            .HasMaxLength(36)
            .IsRequired();

            builder.Property(x => x.IdDay)
            .HasMaxLength(36)
            .IsRequired();

            builder.HasIndex(x => new
            {
                x.IsActive
            }
            )
            .IncludeProperties(x => new
            {
                x.ScheduleDate,
                x.ClassID
            });

            builder.Property(x => x.TeacherName)
           .HasMaxLength(100)
          .IsRequired();

            builder.HasOne(x => x.ScheduleRealization2)
            .WithMany(x => x.HistoryScheduleRealization2)
            .HasForeignKey(fk => fk.IdScheduleRealization2)
            .HasConstraintName("FK_HTrScheduleRealization2_TrScheduleRealization2")
            .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.Venue)
            .WithMany(x => x.HistoryScheduleRealization2)
            .HasForeignKey(fk => fk.IdVenue)
            .HasConstraintName("FK_HTrScheduleRealization2_MsVenue")
            .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.VenueChange)
            .WithMany(x => x.HistoryScheduleRealization2Change)
            .HasForeignKey(fk => fk.IdVenueChange)
            .HasConstraintName("FK_HTrScheduleRealization2_MsVenueChange")
            .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.Staff)
           .WithMany(x => x.HistoryScheduleRealization2)
           .HasForeignKey(fk => fk.IdBinusian)
           .HasConstraintName("FK_HTrScheduleRealization2_MsStaff")
           .OnDelete(DeleteBehavior.NoAction)
             .IsRequired();

            builder.HasOne(x => x.StaffSubtitute)
            .WithMany(x => x.HistoryScheduleRealization2Subtitutes)
            .HasForeignKey(fk => fk.IdBinusianSubtitute)
            .HasConstraintName("FK_HTrScheduleRealization2_MsStaffSubtitute")
            .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.Lesson)
            .WithMany(x => x.HistoryScheduleRealization2)
            .HasForeignKey(fk => fk.IdLesson)
            .HasConstraintName("FK_HTrScheduleRealization2_MsLesson")
            .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.AcademicYear)
            .WithMany(x => x.HistoryScheduleRealization2)
            .HasForeignKey(fk => fk.IdAcademicYear)
            .HasConstraintName("FK_HTrScheduleRealization2_MsAcademicYear")
            .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.Level)
            .WithMany(x => x.HistoryScheduleRealization2)
            .HasForeignKey(fk => fk.IdLevel)
            .HasConstraintName("FK_HTrScheduleRealization2_MsLevel")
            .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.Grade)
            .WithMany(x => x.HistoryScheduleRealization2)
            .HasForeignKey(fk => fk.IdGrade)
            .HasConstraintName("FK_HTrScheduleRealization2_MsGrade")
            .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.Day)
            .WithMany(x => x.HistoryScheduleRealization2)
            .HasForeignKey(fk => fk.IdDay)
            .HasConstraintName("FK_HTrScheduleRealization2_LtDay")
            .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            base.Configure(builder);
        }
    }
}
