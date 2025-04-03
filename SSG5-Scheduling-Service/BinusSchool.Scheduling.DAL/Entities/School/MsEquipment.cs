using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.School
{
    public class MsEquipment : AuditEntity, ISchedulingEntity
    {
        public string IdEquipmentType { get; set; }
        public string EquipmentName { get; set; }
        public string? Description { get; set; }
        public int TotalStockQty { get; set; }
        public int? MaxQtyBorrowing { get; set; }
        public virtual MsEquipmentType EquipmentType { get; set; }
        public virtual ICollection<MsVenueEquipment> VenueEquipments { get; set; }
        public virtual ICollection<TrEquipmentReservation> EquipmentReservations { get; set; }
        public virtual ICollection<HTrEquipmentReservation> EquipmentReservationHistories { get; set; }
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
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
