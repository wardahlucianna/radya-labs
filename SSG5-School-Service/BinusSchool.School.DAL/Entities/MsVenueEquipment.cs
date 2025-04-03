using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsVenueEquipment : AuditEntity, ISchoolEntity
    {
        public string IdVenue { get; set; }
        public string IdEquipment { get; set; }
        public int EquipmentQty { get; set; }
        public virtual MsVenue Venue { get; set; }
        public virtual MsEquipment Equipment { get; set; }
    }

    internal class MsVenueEquipmentConfiguration : AuditEntityConfiguration<MsVenueEquipment>
    {
        public override void Configure(EntityTypeBuilder<MsVenueEquipment> builder)
        {
            builder.HasOne(x => x.Venue)
                .WithMany(x => x.VenueEquipments)
                .HasForeignKey(fk => fk.IdVenue)
                .HasConstraintName("FK_MsVenue_MsVenueEquipment")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Equipment)
                .WithMany(x => x.VenueEquipments)
                .HasForeignKey(fk => fk.IdEquipment)
                .HasConstraintName("FK_MsEquipment_MsVenueEquipment")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }

}
