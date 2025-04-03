using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.School
{
    public class MsVenueMapping : AuditEntity, ISchedulingEntity
    {
        public string IdAcademicYear { get; set; }
        public string IdVenue { get; set; }
        public string IdFloor { get; set; }
        public string IdReservationOwner { get; set; }
        public string IdVenueType { get; set; }
        public bool IsNeedApproval { get; set; }
        public bool IsVenueActive { get; set; }
        public string? Description { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsVenue Venue { get; set; }
        public virtual MsFloor Floor { get; set; }
        public virtual MsReservationOwner ReservationOwner { get; set; }
        public virtual LtVenueType VenueType { get; set; }
        public virtual ICollection<TrVenueReservation> VenueReservations { get; set; }
        public virtual ICollection<MsVenueMappingApproval> VenueMappingApprovals { get; set; }
    }

    internal class MsVenueMappingConfiguration : AuditEntityConfiguration<MsVenueMapping>
    {
        public override void Configure(EntityTypeBuilder<MsVenueMapping> builder)
        {
            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.VenueMappings)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Venue)
                .WithMany(x => x.VenueMappings)
                .HasForeignKey(fk => fk.IdVenue)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Floor)
                .WithMany(x => x.VenueMappings)
                .HasForeignKey(fk => fk.IdFloor)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ReservationOwner)
                .WithMany(x => x.VenueMappings)
                .HasForeignKey(fk => fk.IdReservationOwner)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.VenueType)
                .WithMany(x => x.VenueMappings)
                .HasForeignKey(fk => fk.IdVenueType)
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
