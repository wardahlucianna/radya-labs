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
    public class TrScheduleRealization : AuditEntity, ISchedulingEntity
    {
        public string IdGeneratedScheduleLesson { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string IdBinusian { get; set; }
        public string TeacherName { get; set; }
        public string IdBinusianSubtitute { get; set; }
        public string TeacherNameSubtitute { get; set; }
        public string IdVenue { get; set; }
        public string VenueName { get; set; }
        public string IdVenueChange { get; set; }
        public string VenueNameChange { get; set; }
        public string IdHomeroom { get; set; }
        public string HomeroomName { get; set; }
        public string ClassID { get; set; }
        public string SessionID { get; set; }
        public string DaysOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsCancel { get; set; }
        public bool IsSendEmail { get; set; }
        public string NotesForSubtitutions { get; set; }
        public string Status { get; set; }
        public virtual MsHomeroom Homeroom { get; set; }
        public virtual MsVenue Venue { get; set; }
        public virtual MsStaff Staff { get; set; }
        public virtual MsStaff StaffSubtitute { get; set; }
        public virtual TrGeneratedScheduleLesson GeneratedScheduleLesson { get; set; }
    }

    internal class TrScheduleRealizationConfiguration : AuditEntityConfiguration<TrScheduleRealization>
    {
        public override void Configure(EntityTypeBuilder<TrScheduleRealization> builder)
        {
            builder.Property(x => x.IdGeneratedScheduleLesson)
               .HasMaxLength(36)
               .IsRequired();

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

            builder.Property(x => x.IdHomeroom)
           .HasMaxLength(36)
           .IsRequired();

            builder.Property(x => x.HomeroomName)
            .HasMaxLength(50)
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

            builder.HasIndex(x => new
            {
                x.IsActive
            }
            )
            .IncludeProperties(x => new
            {
                x.IdGeneratedScheduleLesson,
                x.ScheduleDate,
                x.ClassID
            });

            builder.Property(x => x.TeacherName)
           .HasMaxLength(100)
          .IsRequired();

            builder.HasOne(x => x.GeneratedScheduleLesson)
                .WithMany(x => x.ScheduleRealizations)
                .HasForeignKey(fk => fk.IdGeneratedScheduleLesson)
                .HasConstraintName("FK_TrScheduleRealization_TrGeneratedScheduleLesson")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Homeroom)
              .WithMany(x => x.ScheduleRealizations)
              .HasForeignKey(fk => fk.IdHomeroom)
              .HasConstraintName("FK_TrScheduleRealization_MsHomeroom")
              .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Venue)
            .WithMany(x => x.ScheduleRealizations)
            .HasForeignKey(fk => fk.IdVenue)
            .HasConstraintName("FK_TrScheduleRealization_MsVenue")
            .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.Venue)
            .WithMany(x => x.ScheduleRealizations)
            .HasForeignKey(fk => fk.IdVenueChange)
            .HasConstraintName("FK_TrScheduleRealization_MsVenueChange")
            .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.Staff)
            .WithMany(x => x.ScheduleRealizations)
            .HasForeignKey(fk => fk.IdBinusian)
            .HasConstraintName("FK_TrScheduleRealization_MsStaff")
            .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.StaffSubtitute)
            .WithMany(x => x.ScheduleRealizationsSubtitutes)
            .HasForeignKey(fk => fk.IdBinusianSubtitute)
            .HasConstraintName("FK_TrScheduleRealization_MsStaffSubtitute")
            .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            base.Configure(builder);
        }
    }
}
