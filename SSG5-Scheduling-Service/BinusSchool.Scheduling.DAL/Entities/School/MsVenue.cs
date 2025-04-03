using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.School
{
    public class MsVenue : CodeEntity, ISchedulingEntity
    {
        public string IdBuilding { get; set; }
        public virtual MsBuilding Building { get; set; }
        public virtual ICollection<MsHomeroom> Homerooms { get; set; }
        public virtual ICollection<MsSchedule> Schedules { get; set; }
        public virtual ICollection<MsExtracurricularSession> ExtracurricularSessions { get; set; }
        public virtual ICollection<TrGeneratedScheduleLesson> GeneratedScheduleLessons { get; set; }
        public virtual ICollection<TrInvitationBooking> InvitationBookings { get; set; }
        public virtual ICollection<TrInvitationBookingSettingVenueDtl> InvitationBookingSettingVenueDtls { get; set; }
        public virtual ICollection<TrInvitationBookingSettingSchedule> InvitationBookingSettingSchedules { get; set; }
        public virtual ICollection<TrPersonalInvitation> PersonalInvitations { get; set; }
        public virtual ICollection<TrVisitorSchool> VisitorSchools { get; set; }
        public virtual ICollection<TrScheduleRealization> ScheduleRealizations { get; set; }
        public virtual ICollection<TrScheduleRealization2> ScheduleRealization2s { get; set; }
        public virtual ICollection<HTrScheduleRealization2> HistoryScheduleRealization2 { get; set; }
        public virtual ICollection<HTrScheduleRealization2> HistoryScheduleRealization2Change { get; set; }
        public virtual ICollection<TrScheduleRealization2> ScheduleRealization2sChange { get; set; }
        public virtual ICollection<MsScheduleLesson> ScheduleLesson { get; set; }
        public virtual ICollection<MsVenueMapping> VenueMappings { get; set; }
        public virtual ICollection<MsVenueEquipment> VenueEquipments { get; set; }
        public virtual ICollection<TrMappingEquipmentReservation> MappingEquipmentReservations { get; set; }
    }

    internal class MsVenueConfiguration : CodeEntityConfiguration<MsVenue>
    {
        public override void Configure(EntityTypeBuilder<MsVenue> builder)
        {
            builder.HasOne(x => x.Building)
            .WithMany(x => x.Venues)
            .HasForeignKey(fk => fk.IdBuilding)
            .HasConstraintName("FK_MsVenue_MsBuilding")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

            base.Configure(builder);
        }
    }
}
