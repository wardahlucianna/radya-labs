using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrInvitationBookingDetail : AuditEntity, ISchedulingEntity
    {
        public string IdHomeroomStudent { get; set; }
        public string IdInvitationBooking { get; set; }
        public virtual MsHomeroomStudent HomeroomStudent { get; set; }
        public virtual TrInvitationBooking InvitationBooking { get; set; }
        
    }

    internal class TrInvitationBookingDetailConfiguration : AuditEntityConfiguration<TrInvitationBookingDetail>
    {
        public override void Configure(EntityTypeBuilder<TrInvitationBookingDetail> builder)
        {
            builder.Property(x => x.IdHomeroomStudent)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdInvitationBooking)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.HomeroomStudent)
                .WithMany(x => x.InvitationBookingDetails)
                .HasForeignKey(fk => fk.IdHomeroomStudent)
                .HasConstraintName("FK_TrInvitationBookingDetail_MsHomeroomStudent")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.InvitationBooking)
               .WithMany(x => x.InvitationBookingDetails)
               .HasForeignKey(fk => fk.IdInvitationBooking)
               .HasConstraintName("FK_TrInvitationBookingDetail_MsVenue")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
