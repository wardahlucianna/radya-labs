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
    public class TrEquipmentReservation : AuditEntity, ISchedulingEntity
    {
        public string IdMappingEquipmentReservation { get; set; }
        public string IdEquipment { get; set; }
        public int EquipmentBorrowingQty { get; set; }
        public virtual TrMappingEquipmentReservation MappingEquipmentReservation { get; set; }
        public virtual MsEquipment Equipment { get; set; }
        public virtual ICollection<HTrEquipmentReservation> EquipmentReservations { get; set; }
    }

    internal class TrEquipmentReservationConfiguration : AuditEntityConfiguration<TrEquipmentReservation>
    {
        public override void Configure(EntityTypeBuilder<TrEquipmentReservation> builder)
        {
            builder.HasOne(x => x.MappingEquipmentReservation)
                .WithMany(x => x.EquipmentReservations)
                .HasForeignKey(fk => fk.IdMappingEquipmentReservation)
                .HasConstraintName("FK_TrMappingEquipmentReservation_TrEquipmentReservation")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Equipment)
                .WithMany(x => x.EquipmentReservations)
                .HasForeignKey(fk => fk.IdEquipment)
                .HasConstraintName("FK_MsEquipment_TrEquipmentReservation")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
