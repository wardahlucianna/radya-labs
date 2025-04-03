using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrInvitationBookingSettingSchedule : AuditEntity, ISchedulingEntity
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Description { get; set; }
        public int QuotaSlot { get; set; }
        public int Duration { get; set; }
        public bool? IsPriority { get; set; }
        public bool? IsFlexibleBreak { get; set; }
        public bool IsFixedBreak { get; set; }
        public DateTime DateInvitation { get; set; }
        public bool IsAvailable { get; set; }
        public string IdInvitationBookingSetting { get; set; }
        public string IdUserTeacher { get; set; }
        public string IdInvitationBookingSettingBreak { get; set; }
        public string BreakName { get; set; }
        public string IdVenue { get; set; }
        public string IdUserSetPriority { get; set; }
        public bool IsDisabledAvailable { get; set; }
        public bool IsDisabledPriority { get; set; }
        public bool IsDisabledFlexible { get; set; }
        public virtual TrInvitationBookingSetting InvitationBookingSetting { get; set; }
        public virtual MsUser UserTeacher { get; set; }
        public virtual MsUser UserSetPriority { get; set; }
        public virtual MsVenue Venue { get; set; }
    }

    internal class TrInvitationBookingSettingScheduleConfiguration : AuditEntityConfiguration<TrInvitationBookingSettingSchedule>
    {
        public override void Configure(EntityTypeBuilder<TrInvitationBookingSettingSchedule> builder)
        {
            builder.Property(x => x.Description)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.IdInvitationBookingSettingBreak)
               .HasMaxLength(36);

            builder.Property(x => x.BreakName)
               .HasMaxLength(100);

            builder.Property(x => x.IsPriority);

            builder.Property(x => x.IsFlexibleBreak);

            builder.HasOne(x => x.InvitationBookingSetting)
               .WithMany(x => x.InvitationBookingSettingSchedules)
               .HasForeignKey(fk => fk.IdInvitationBookingSetting)
               .HasConstraintName("FK_TrInvitationBookingSettingSchedule_TrInvitationBookingSetting")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.HasOne(x => x.UserTeacher)
               .WithMany(x => x.InvitationBookingSettingSchedulesUser)
               .HasForeignKey(fk => fk.IdUserTeacher)
               .HasConstraintName("FK_TrInvitationBookingSettingSchedule_MsUserTeacher")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.HasOne(x => x.UserSetPriority)
              .WithMany(x => x.InvitationBookingSettingSchedulesSetUserPriority)
              .HasForeignKey(fk => fk.IdUserSetPriority)
              .HasConstraintName("FK_TrInvitationBookingSettingSchedule_MsUserSetPriority")
              .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Venue)
              .WithMany(x => x.InvitationBookingSettingSchedules)
              .HasForeignKey(fk => fk.IdVenue)
              .HasConstraintName("FK_TrInvitationBookingSettingSchedule_MsVenue")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            base.Configure(builder);
        }
    }
}
