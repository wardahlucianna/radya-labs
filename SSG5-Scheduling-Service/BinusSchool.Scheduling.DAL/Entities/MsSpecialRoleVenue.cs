using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsSpecialRoleVenue : AuditEntity, ISchedulingEntity
    {
        public string IdRole { get; set; }
        public int SpecialDurationBookingTotalDay { get; set; }
        public bool CanOverrideAnotherReservation { get; set; }
        public bool AllSuperAccess { get; set; }
        public virtual LtRole Role { get; set; }

    }
    internal class MsSpecialRoleVenueConfiguration : AuditEntityConfiguration<MsSpecialRoleVenue>
    {
        public override void Configure(EntityTypeBuilder<MsSpecialRoleVenue> builder)
        {
            builder.HasOne(x => x.Role)
            .WithMany(x => x.SpecialRoleVenues)
            .HasForeignKey(fk => fk.IdRole)
            .HasConstraintName("FK_LtRole_MsSpecialRoleVenue")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

            base.Configure(builder);
        }
    }
}
