using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrInvitationBooking : AuditEntity, ISchedulingEntity
    {
        public TrInvitationBooking()
        {
            StatusData = InvitatinBookingStatusData.OnProgress;
        }

        public string IdVenue { get; set; }
        public string IdInvitationBookingSetting { get; set; }
        public string IdUserTeacher { get; set; }
        public DateTime StartDateInvitation { get; set; }
        public DateTime EndDateInvitation { get; set; }
        public InvitatinBookingStatusData StatusData { get; set; }
        public string StatusDataMessage { get; set; }
        public string Description { get; set; }
        public virtual MsVenue Venue { get; set; }
        public virtual MsUser UserTeacher { get; set; }
        public virtual TrInvitationBookingSetting InvitationBookingSetting { get; set; }
        public virtual ICollection<TrInvitationBookingDetail> InvitationBookingDetails { get; set; }
        public InvitationBookingStatus Status { get; set; }
        public InvitationBookingInitiateBy InitiateBy { get; set; }
        public string Note { get; set; }
    }

    internal class TrInvitationBookingConfiguration : AuditEntityConfiguration<TrInvitationBooking>
    {
        public override void Configure(EntityTypeBuilder<TrInvitationBooking> builder)
        {
            builder.HasQueryFilter(e => e.StatusData == InvitatinBookingStatusData.Success);
            builder.Property(x => x.Description)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.IdVenue)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.Description)
               .HasMaxLength(100)
               .IsRequired();

            builder.Property(e => e.StatusData).HasMaxLength(maxLength: 50)
               .HasConversion(valueToDb =>
               valueToDb.ToString(),
               valueFromDb => (InvitatinBookingStatusData)Enum.Parse(typeof(InvitatinBookingStatusData), valueFromDb));

            builder.Property(x => x.StatusDataMessage)
                .HasMaxLength(100);

            builder.Property(e => e.Status).HasMaxLength(maxLength: 10)
               .HasConversion(valueToDb =>
               valueToDb.ToString(),
               valueFromDb => (InvitationBookingStatus)Enum.Parse(typeof(InvitationBookingStatus), valueFromDb));

            builder.Property(e => e.InitiateBy).HasMaxLength(maxLength: 10)
               .HasConversion(valueToDb =>
               valueToDb.ToString(),
               valueFromDb => (InvitationBookingInitiateBy)Enum.Parse(typeof(InvitationBookingInitiateBy), valueFromDb));

            builder.Property(x => x.Note)
                .HasMaxLength(450);

            builder.Property(x => x.IdInvitationBookingSetting)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdUserTeacher)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.StartDateInvitation)
                .IsRequired();

            builder.HasOne(x => x.Venue)
               .WithMany(x => x.InvitationBookings)
               .HasForeignKey(fk => fk.IdVenue)
               .HasConstraintName("FK_TrInvitationBooking_MsVenue")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

            builder.HasOne(x => x.UserTeacher)
              .WithMany(x => x.InvitationBookings)
              .HasForeignKey(fk => fk.IdUserTeacher)
              .HasConstraintName("FK_TrInvitationBooking_MsUserTeacher")
              .OnDelete(DeleteBehavior.Cascade)
              .IsRequired();

            builder.HasOne(x => x.InvitationBookingSetting)
              .WithMany(x => x.InvitationBookings)
              .HasForeignKey(fk => fk.IdInvitationBookingSetting)
              .HasConstraintName("FK_TrInvitationBooking_TrInvitationBookingSetting")
              .OnDelete(DeleteBehavior.Cascade)
              .IsRequired();

            base.Configure(builder);
        }
    }
}
