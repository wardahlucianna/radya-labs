using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrInvitationBookingSettingVenueDate : AuditEntity, ISchedulingEntity
    {
        public DateTime DateInvitationExact { get; set; }
        public string DateInvitation { get; set; }
        public string IdInvitationBookingSetting { get; set; }
        public virtual TrInvitationBookingSetting InvitationBookingSetting { get; set; }
        public virtual ICollection<TrInvitationBookingSettingVenueDtl> InvitationBookingSettingVenueDtl { get; set; }
    }

    internal class TrInvitationBookingSettingVenueDateConfiguration : AuditEntityConfiguration<TrInvitationBookingSettingVenueDate>
    {
        public override void Configure(EntityTypeBuilder<TrInvitationBookingSettingVenueDate> builder)
        {
            builder.Property(x => x.DateInvitation)
               .HasMaxLength(1045)
               .IsRequired();


            builder.HasOne(x => x.InvitationBookingSetting)
               .WithMany(x => x.InvitationBookingSettingVenueDates)
               .HasForeignKey(fk => fk.IdInvitationBookingSetting)
               .HasConstraintName("FK_TrInvitationBookingSettingVenueDate_TrInvitationBookingSetting")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
