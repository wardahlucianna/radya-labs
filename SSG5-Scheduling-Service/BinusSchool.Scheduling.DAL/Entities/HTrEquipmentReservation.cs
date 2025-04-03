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
    public class HTrEquipmentReservation : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string IdHTrEquipmentReservation { get; set; }
        public string IdEquipmentReservation { get; set; }
        public string IdMappingEquipmentReservation { get; set; }
        public string IdEquipment { get; set; }
        public int EquipmentBorrowingQty { get; set; }
        public string IdHTrMappingEquipmentReservation { get; set; }
        public virtual TrEquipmentReservation EquipmentReservation { get; set; }
        public virtual MsEquipment Equipment { get; set; }
    }

    internal class HTrEquipmentReservationConfiguration : AuditNoUniqueEntityConfiguration<HTrEquipmentReservation>
    {
        public override void Configure(EntityTypeBuilder<HTrEquipmentReservation> builder)
        {
            builder.HasKey(x => x.IdHTrEquipmentReservation);

            builder.Property(p => p.IdHTrEquipmentReservation)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.IdHTrMappingEquipmentReservation)
                .HasMaxLength(36);

            builder.HasOne(x => x.EquipmentReservation)
                .WithMany(x => x.EquipmentReservations)
                .HasForeignKey(fk => fk.IdEquipmentReservation)
                .HasConstraintName("FK_TrEquipmentReservation_HTrEquipmentReservation")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Equipment)
                .WithMany(a => a.EquipmentReservationHistories)
                .HasForeignKey(fk => fk.IdEquipment)
                .HasConstraintName("FK_HTrEquipmentReservation_MsEquipment")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
