using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrMappingEquipmentReservation : AuditEntity, ISchedulingEntity
    {
        public DateTime ScheduleStartDate { get; set; }
        public DateTime ScheduleEndDate { get; set; }
        public string IdUser { get; set; }
        public string? IdVenue { get; set; }
        public string EventDescription { get; set; }
        public string? IdVenueReservation { get; set; }
        public string? VenueNameinEquipment { get; set; }
        public string? Notes { get; set; }
        public virtual MsUser User { get; set; }
        public virtual MsVenue Venue { get; set; }
        public virtual TrVenueReservation VenueReservation { get; set; }
        public virtual ICollection<TrEquipmentReservation> EquipmentReservations { get; set; }
        public virtual ICollection<HTrMappingEquipmentReservation> MappingEquipmentReservations { get; set; }
    }

    internal class TrMappingEquipmentReservationConfiguration : AuditEntityConfiguration<TrMappingEquipmentReservation>
    {
        public override void Configure(EntityTypeBuilder<TrMappingEquipmentReservation> builder)
        {
            builder.HasOne(x => x.User)
                .WithMany(x => x.MappingEquipmentReservations)
                .HasForeignKey(fk => fk.IdUser)
                .HasConstraintName("FK_MsUser_TrMappingEquipmentReservation")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Venue)
                .WithMany(x => x.MappingEquipmentReservations)
                .HasForeignKey(fk => fk.IdVenue)
                .HasConstraintName("FK_MsVenue_TrMappingEquipmentReservation")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.VenueReservation)
                .WithMany(x => x.MappingEquipmentReservations)
                .HasForeignKey(fk => fk.IdVenueReservation)
                .HasConstraintName("FK_TrVenueReservation_TrMappingEquipmentReservation")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
