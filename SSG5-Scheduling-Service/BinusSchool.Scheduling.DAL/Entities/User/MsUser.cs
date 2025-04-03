using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.User
{
    public class MsUser : AuditEntity, ISchedulingEntity
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public bool IsActiveDirectory { get; set; }
        public bool Status { get; set; }
        public virtual ICollection<MsUserRole> UserRoles { get; set; }
        public virtual ICollection<MsUserSchool> UserSchools { get; set; }
        public virtual ICollection<TrEventCoordinator> EventCoordinators { get; set; }
        public virtual ICollection<HTrEventCoordinator> HistoryEventCoordinators { get; set; }
        public virtual ICollection<TrEventApprover> EventApprovers { get; set; }
        public virtual ICollection<HTrEventApprover> HistoryEventApprovers { get; set; }
        public virtual ICollection<TrEventIntendedForPersonal> EventIntendedForPersonals { get; set; }
        public virtual ICollection<HTrEventIntendedForPersonal> HistoryEventIntendedForPersonals { get; set; }
        public virtual ICollection<TrEventIntendedForAtdPICStudent> TrEventIntendedForAtdPICStudents { get; set; }
        public virtual ICollection<HTrEventIntendedForAtdPICStudent> HTrEventIntendedForAtdPICStudent { get; set; }
        public virtual ICollection<TrUserEvent> TrUserEvents { get; set; }
        public virtual ICollection<HTrUserEvent> HistoryUserEvents { get; set; }
        public virtual ICollection<TrEventActivityPIC> EventActivityPICs { get; set; }
        public virtual ICollection<HTrEventActivityPIC> HistoryEventActivityPICs { get; set; }
        public virtual ICollection<TrEventActivityRegistrant> EventActivityRegistrants { get; set; }
        public virtual ICollection<HTrEventActivityRegistrant> HistoryEventActivityRegistrants { get; set; }
        public virtual ICollection<MsEventApproverSetting> Approver1 { get; set; }
        public virtual ICollection<MsEventApproverSetting> Approver2 { get; set; }
        public virtual ICollection<MsCertificateTemplate> Signature1 { get; set; }
        public virtual ICollection<MsCertificateTemplate> Signature2 { get; set; }
        public virtual ICollection<HTrEventApproval> EventApprovals { get; set; }
        public virtual ICollection<TrEventAwardApprover> EventAwardApprovers { get; set; }
        public virtual ICollection<HTrEventAwardApprover> HistoryEventAwardApprovers { get; set; }
        public virtual ICollection<MsCertificateTemplateApprover> CertificateTemplateApprovers { get; set; }
        public virtual ICollection<HMsCertificateTemplateApprover> HistoryCertificateTemplateApprovers { get; set; }
        public virtual ICollection<TrNonTeachingLoad> NonTeachingLoads { get; set; }
        public virtual ICollection<MsHomeroomOfficer> HomeroomCaptains { get; set; }
        public virtual ICollection<MsHomeroomOfficer> HomeroomViceCaptains { get; set; }
        public virtual ICollection<MsHomeroomOfficer> HomeroomSecretaries { get; set; }
        public virtual ICollection<MsHomeroomOfficer> HomeroomTreasurers { get; set; }
        public virtual ICollection<MsUserPlatform> UserPlatforms { get; set; }
        public virtual ICollection<TrGeneratedScheduleLesson> GeneratedScheduleLessons { get; set; }
        public virtual ICollection<TrInvitationBooking> InvitationBookings { get; set; }
        public virtual ICollection<TrInvitationBookingSettingVenueDtl> InvitationBookingSettingVenueDtls { get; set; }
        public virtual ICollection<TrInvitationBookingSettingSchedule> InvitationBookingSettingSchedulesUser { get; set; }
        public virtual ICollection<TrInvitationBookingSettingSchedule> InvitationBookingSettingSchedulesSetUserPriority { get; set; }
        public virtual ICollection<TrAvailabilitySetting> AvailabilitySettings { get; set; }
        public virtual ICollection<TrPersonalInvitation> PersonalInvitations { get; set; }
        public virtual ICollection<TrPersonalInvitation> PersonalInvitationsTeacher { get; set; }
        public virtual ICollection<TrVisitorSchool> VisitorSchoolsBook { get; set; }
        public virtual ICollection<TrVisitorSchool> VisitorSchoolsVisitor { get; set; }
        public virtual ICollection<MsExtracurricularExternalCoach> ExtracurricularExternalCoachs { get; set; }
        public virtual ICollection<TrClassDiary> ClassDiaries { get; set; }

        // public virtual ICollection<TrScheduleRealization> ScheduleRealizations { get; set; }
        public virtual ICollection<TrVenueReservation> VenueReservations { get; set; }

        public virtual ICollection<TrMappingEquipmentReservation> MappingEquipmentReservations { get; set; }

        internal class MsUserConfiguration : AuditEntityConfiguration<MsUser>
        {
            public override void Configure(EntityTypeBuilder<MsUser> builder)
            {
                builder.Property(x => x.DisplayName)
               .HasMaxLength(100)
               .IsRequired();

                builder.Property(x => x.Username)
                .HasMaxLength(50)
                .IsRequired();

                builder.Property(x => x.Email).HasMaxLength(128);

                base.Configure(builder);
            }
        }
    }
}
