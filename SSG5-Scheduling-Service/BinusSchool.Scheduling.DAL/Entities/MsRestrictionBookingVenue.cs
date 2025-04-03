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
    public class MsRestrictionBookingVenue : AuditEntity, ISchedulingEntity
    {
        public DateTime StartRestrictionDate { get; set; }
        public DateTime EndRestrictionDate { get; set; }
        public string IdBuilding { get; set; }
        public string IdVenue { get; set; }
        public string IdGroupRestriction { get; set; }
        public virtual MsBuilding Building { get; set; }
    }

    internal class MsRestrictionBookingVenueConfiguration : AuditEntityConfiguration<MsRestrictionBookingVenue>
    {
        public override void Configure(EntityTypeBuilder<MsRestrictionBookingVenue> builder)
        {
            builder.HasOne(x => x.Building)
                .WithMany(x => x.RestrictionBookingVenues)
                .HasForeignKey(fk => fk.IdBuilding)
                .HasConstraintName("FK_MsBuilding_MsRestrictionBookingVenue")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
