using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.School
{
    public class MsVenueReservationRule : AuditEntity, ISchedulingEntity
    {
        public string IdSchool { get; set; }
        public int? MaxDayBookingVenue { get; set; }
        public TimeSpan? MaxTimeBookingVenue { get; set; }
        public int? MaxDayDurationBookingVenue { get; set; }
        public string? VenueNotes { get; set; }
        public TimeSpan StartTimeOperational { get; set; }
        public TimeSpan EndTimeOperational { get; set; }
        public bool CanBookingAnotherUser { get; set; }
        public virtual MsSchool School { get; set; }
    }

    internal class MsVenueReservationRuleConfiguration : AuditEntityConfiguration<MsVenueReservationRule>
    {
        public override void Configure(EntityTypeBuilder<MsVenueReservationRule> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.VenueReservationRules)
                .HasForeignKey(fk => fk.IdSchool)
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
