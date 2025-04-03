using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class HMsEquipment : AuditNoUniqueEntity, ISchoolEntity
    {
        public string IdHMsEquipment { get; set; }
        public string IdEquipment { get; set; }
        public string IdEquipmentType { get; set; }
        public string EquipmentName { get; set; }
        public string? Description { get; set; }
        public int TotalStockQty { get; set; }
        public int? MaxQtyBorrowing { get; set; }
        public virtual MsEquipment Equipment { get; set; }
    }

    internal class HMsEquipmentConfiguration : AuditNoUniqueEntityConfiguration<HMsEquipment>
    {
        public override void Configure(EntityTypeBuilder<HMsEquipment> builder)
        {
            builder.HasKey(x => x.IdHMsEquipment);

            builder.Property(x => x.IdHMsEquipment)
               .HasMaxLength(36)
               .IsRequired();

            builder.Property(x => x.EquipmentName)
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.HasOne(x => x.Equipment)
                .WithMany(x => x.Equipments)
                .HasForeignKey(fk => fk.IdEquipment)
                .HasConstraintName("FK_MsEquipment_HMsVenueEquipment")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
