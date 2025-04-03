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
    public class TrVenueReservationApproval : AuditEntity, ISchedulingEntity
    {
        public string IdVenueReservation { get; set; }
        public int Status { get; set; }
        public string IdVenueMappingApproval { get; set; }
        public string RejectionReason { get; set; }
        public virtual TrVenueReservation VenueReservation { get; set; }
        public virtual MsVenueMappingApproval VenueMappingApproval { get; set; }
    }

    internal class TrVenueReservationApprovalConfiguration : AuditEntityConfiguration<TrVenueReservationApproval>
    {
        public override void Configure(EntityTypeBuilder<TrVenueReservationApproval> builder)
        {
            //builder.HasOne(x => x.VenueReservation)
            //    .WithMany(x => x.VenueReservationApprovals)
            //    .HasForeignKey(fk => fk.IdVenueReservation)
            //    .HasConstraintName("FK_TrVenueReservation_TrVenueReservationApproval")
            //    .OnDelete(DeleteBehavior.NoAction)
            //    .IsRequired();

            //builder.HasOne(x => x.VenueMappingApproval)
            //    .WithMany(x => x.VenueReservationApprovals)
            //    .HasForeignKey(fk => fk.IdVenueMappingApproval)
            //    .HasConstraintName("FK_MsVenueMappingApproval_TrVenueReservationApproval")
            //    .OnDelete(DeleteBehavior.Cascade)
            //    .IsRequired();

            base.Configure(builder);
        }
    }
}
