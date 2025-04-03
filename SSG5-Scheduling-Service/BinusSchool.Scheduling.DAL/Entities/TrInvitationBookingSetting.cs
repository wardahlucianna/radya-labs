using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrInvitationBookingSetting : AuditEntity, ISchedulingEntity
    {
        public string IdAcademicYear { get; set; }
        public string InvitationName { get; set; }
        public DateTime InvitationStartDate { get; set; }
        public DateTime InvitationEndDate { get; set; }
        public InvitationType InvitationType { get; set; }
        public DateTime ParentBookingStartDate { get; set; }
        public DateTime ParentBookingEndDate { get; set; }
        // public string ParentBookingStartTime { get; set; }
        // public string ParentBookingEndTime { get; set; }
        public DateTime? StaffBookingStartDate { get; set; }
        public DateTime? StaffBookingEndDate { get; set; }
        // public string StaffBookingStartTime { get; set; }
        // public string StaffBookingEndTime { get; set; }
        public bool SchedulingSiblingSameTime { get; set; }
        public string FootNote { get; set; }
        public int StepWizard { get; set; }
        public StatusInvitationBookingSetting Status { get; set; }
        public virtual MsAcademicYear AcademicYears { get; set; }
        public virtual ICollection<TrInvitationBookingSettingDetail> InvitationBookingSettingDetails { get; set; }
        public virtual ICollection<TrInvitationBooking> InvitationBookings { get; set; }
        public virtual ICollection<TrInvitationBookingSettingUser> InvitationBookingSettingUsers { get; set; }
        public virtual ICollection<TrInvitationBookingSettingBreak> InvitationBookingSettingBreaks { get; set; }
        public virtual ICollection<TrInvitationBookingSettingSchedule> InvitationBookingSettingSchedules { get; set; }
        public virtual ICollection<TrInvitationBookingSettingVenueDate> InvitationBookingSettingVenueDates { get; set; }
        public virtual ICollection<TrInvitationEmail> InvitationEmails { get; set; }
        public virtual ICollection<TrInvitationBookingSettingQuota> InvitationBookingSettingQuotas { get; set; }
        public virtual ICollection<TrInvBookingSettingRoleParticipant> InvBookingSettingRoleParticipants { get; set; }
        public virtual ICollection<TrInvitationBookingSettingExcludeSub> InvBookingSettingExcludeSub { get; set; }
    }

    internal class TrInvitationBookingSettingConfiguration : AuditEntityConfiguration<TrInvitationBookingSetting>
    {
        public override void Configure(EntityTypeBuilder<TrInvitationBookingSetting> builder)
        {
            builder.HasOne(x => x.AcademicYears)
              .WithMany(x => x.TrInvitationBookingSettings)
              .HasForeignKey(fk => fk.IdAcademicYear)
              .HasConstraintName("FK_TrInvitationBookingSetting_MsAcademicYear")
              .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.Property(x => x.InvitationName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.FootNote)
                .HasMaxLength(450);

            builder.Property(e => e.InvitationType)
                .HasMaxLength(maxLength: 10)
                .HasConversion(valueToDb =>
                valueToDb.ToString(),
                valueFromDb => (InvitationType)Enum.Parse(typeof(InvitationType), valueFromDb))
                .IsRequired();

            builder.Property(e => e.Status)
                .HasMaxLength(maxLength: 20)
                .HasConversion(valueToDb =>
                valueToDb.ToString(),
                valueFromDb => (StatusInvitationBookingSetting)Enum.Parse(typeof(StatusInvitationBookingSetting), valueFromDb))
                .IsRequired();
                
            base.Configure(builder);
        }
    }
}
