using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrVenueReservation : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
        public string IdVenueReservation { get; set; }
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
        public virtual TrVenueReservation VenueReservation { get; set; }
    }

    internal class HTrVenueReservationConfiguration : AuditNoUniqueEntityConfiguration<HTrVenueReservation>
    {
        public override void Configure(EntityTypeBuilder<HTrVenueReservation> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("IdHTrVenueReservation")
                .HasMaxLength(36);

            builder.HasOne(x => x.VenueReservation)
                .WithMany(x => x.VenueReservations)
                .HasForeignKey(fk => fk.IdVenueReservation)
                .HasConstraintName("FK_TrVenueReservation_HTrVenueReservation")
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
