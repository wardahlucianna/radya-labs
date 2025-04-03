using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrVenueReservation : AuditEntity, ISchedulingEntity
    {
        public string IdVenueMapping { get; set; }
        public DateTime ScheduleDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string IdUser { get; set; }
        public string EventDescription { get; set; }
        public string IdRepeatGroup { get; set; }
        public string URL { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public decimal FileSize { get; set; }
        public string Notes { get; set; }
        public int? CleanUpTime { get; set; }
        public int? PreparationTime { get; set; }
        public bool IsOverlapping { get; set; }
        public int Status { get; set; }
        public string RejectionReason { get; set; }
        public string IdUserAction { get; set; }

        public virtual MsVenueMapping VenueMapping { get; set; }
        public virtual MsUser User { get; set; }
        public virtual ICollection<HTrVenueReservation> VenueReservations { get; set; }
        public virtual ICollection<TrMappingEquipmentReservation> MappingEquipmentReservations { get; set; }
    }

    internal class TrVenueReservationConfiguration : AuditEntityConfiguration<TrVenueReservation>
    {
        public override void Configure(EntityTypeBuilder<TrVenueReservation> builder)
        {
            builder.HasOne(x => x.VenueMapping)
                .WithMany(x => x.VenueReservations)
                .HasForeignKey(fk => fk.IdVenueMapping)
                .HasConstraintName("FK_MsVenueMapping_TrVenueReservation")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.VenueReservations)
                .HasForeignKey(fk => fk.IdUser)
                .HasConstraintName("FK_MsUser_TrVenueReservation")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Property(x => x.RejectionReason)
                .HasMaxLength(200);

            builder.Property(x => x.FileSize)
                .HasColumnType("decimal(18,2)");

            base.Configure(builder);
        }
    }
}
