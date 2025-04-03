using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsVenueMapping : AuditEntity, ISchoolEntity
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
        public virtual ICollection<MsVenueMappingApproval> VenueMappingApprovals { get; set; }
    }

    internal class MsVenueMappingConfiguration : AuditEntityConfiguration<MsVenueMapping>
    {
        public override void Configure(EntityTypeBuilder<MsVenueMapping> builder)
        {
            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.VenueMappings)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsAcademicYear_MsVenueMapping")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Venue)
                .WithMany(x => x.VenueMappings)
                .HasForeignKey(fk => fk.IdVenue)
                .HasConstraintName("FK_MsVenue_MsVenueMapping")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Floor)
                .WithMany(x => x.VenueMappings)
                .HasForeignKey(fk => fk.IdFloor)
                .HasConstraintName("FK_MsFloor_MsVenueMapping")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.ReservationOwner)
                .WithMany(x => x.VenueMappings)
                .HasForeignKey(fk => fk.IdReservationOwner)
                .HasConstraintName("FK_MsReservationOwner_MsVenueMapping")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.VenueType)
                .WithMany(x => x.VenueMappings)
                .HasForeignKey(fk => fk.IdVenueType)
                .HasConstraintName("FK_LtVenueType_MsVenueMapping")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
