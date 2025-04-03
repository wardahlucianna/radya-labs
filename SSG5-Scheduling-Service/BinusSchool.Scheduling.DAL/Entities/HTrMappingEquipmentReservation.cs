using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrMappingEquipmentReservation : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string IdHTrMappingEquipmentReservation { get; set; }
        public string IdMappingEquipmentReservation { get; set; }
        public DateTime ScheduleStartDate { get; set; }
        public DateTime ScheduleEndDate { get; set; }
        public string IdUser { get; set; }
        public string? IdVenue { get; set; }
        public string? VenueNameinEquipment { get; set; }
        public string EventDescription { get; set; }
        public string? IdVenueReservation { get; set; }
        public string? Notes { get; set; }
        public virtual TrMappingEquipmentReservation MappingEquipmentReservation { get; set; }
    }

    internal class HTrMappingEquipmentReservationConfiguration : AuditNoUniqueEntityConfiguration<HTrMappingEquipmentReservation>
    {
        public override void Configure(EntityTypeBuilder<HTrMappingEquipmentReservation> builder)
        {
            builder.HasKey(x => x.IdHTrMappingEquipmentReservation);

            builder.Property(p => p.IdHTrMappingEquipmentReservation)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.MappingEquipmentReservation)
                .WithMany(x => x.MappingEquipmentReservations)
                .HasForeignKey(fk => fk.IdMappingEquipmentReservation)
                .HasConstraintName("FK_TrMappingEquipmentReservation_HTrMappingEquipmentReservation")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
