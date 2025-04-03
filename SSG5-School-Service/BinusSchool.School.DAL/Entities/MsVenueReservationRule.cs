using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsVenueReservationRule : AuditEntity, ISchoolEntity
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
                .HasConstraintName("FK_MsSchool_MsVenueReservationRule")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
