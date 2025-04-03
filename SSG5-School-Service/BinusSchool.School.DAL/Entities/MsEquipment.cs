using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsEquipment : AuditEntity, ISchoolEntity
    {
        public string IdEquipmentType { get; set; }
        public string EquipmentName { get; set; }
        public string? Description { get; set; }
        public int TotalStockQty { get; set; }
        public int? MaxQtyBorrowing { get; set; }
        public virtual MsEquipmentType EquipmentType { get; set; }
        public virtual ICollection<MsVenueEquipment> VenueEquipments { get; set; }
        public virtual ICollection<HMsEquipment> Equipments { get; set; }
    }

    internal class MsEquipmentConfiguration : AuditEntityConfiguration<MsEquipment>
    {
        public override void Configure(EntityTypeBuilder<MsEquipment> builder)
        {
            builder.Property(x => x.EquipmentName)
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.HasOne(x => x.EquipmentType)
                .WithMany(x => x.Equipments)
                .HasForeignKey(fk => fk.IdEquipmentType)
                .HasConstraintName("FK_MsEquipmentType_MsEquipment")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
