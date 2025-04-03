using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.User
{
    public class MsUser : AuditEntity, IStudentEntity
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public bool IsActiveDirectory { get; set; }
        public bool Status { get; set; }

        //public virtual ICollection<MsUserRole> UserRoles { get; set; }
        //public virtual ICollection<MsUserSchool> UserSchools { get; set; }
        //public virtual ICollection<MsEventIntendedForAttendancePICStudent> EventIntendedForAttendancePICStudents { get; set; }
        //public virtual ICollection<MsUserEvent> UserEvents { get; set; }
        //public virtual ICollection<TrEvent> EventCoordinator { get; set; }
        //public virtual ICollection<TrEventApprover> EventApprovers { get; set; }
        //public virtual ICollection<TrEventIntendedForPersonal> EventIntendedForPersonals { get; set; }
        //public virtual ICollection<TrEventIntendedForAttendancePICStudent> TrEventIntendedForAttendancePICStudents { get; set; }
        //public virtual ICollection<TrUserEvent> TrUserEvents { get; set; }
        //public virtual ICollection<TrEventActivityPIC> EventActivityPICs { get; set; }
        //public virtual ICollection<TrEventActivityRegistrant> EventActivityRegistrants { get; set; }
        //public virtual ICollection<MsEventApproverSetting> Approver1 { get; set; }
        //public virtual ICollection<MsEventApproverSetting> Approver2 { get; set; }
        //public virtual ICollection<MsCertificateTemplate> Signature1 { get; set; }
        //public virtual ICollection<MsCertificateTemplate> Signature2 { get; set; }
        //public virtual ICollection<HsEventApproval> EventApprovals { get; set; }
        //public virtual ICollection<TrEventAwardApprover> EventAwardApprovers { get; set; }
        //public virtual ICollection<MsCertificateTemplateApprover> CertificateTemplateApprovers { get; set; }
        //public virtual ICollection<HsCertificateTemplateApprover> HistoryCertificateTemplateApprovers { get; set; }
        public virtual ICollection<TrStudentMeritApprovalHs> StudentMeritApprovals1 { get; set; }
        public virtual ICollection<TrStudentMeritApprovalHs> StudentMeritApprovals2 { get; set; }
        public virtual ICollection<TrStudentMeritApprovalHs> StudentMeritApprovals3 { get; set; }
        public virtual ICollection<TrStudentDemeritApprovalHs> StudentDemeritApprovals1 { get; set; }
        public virtual ICollection<TrStudentDemeritApprovalHs> StudentDemeritApprovals2 { get; set; }
        public virtual ICollection<TrStudentDemeritApprovalHs> StudentDemeritApprovals3 { get; set; }
        public virtual ICollection<TrNonTeachingLoad> NonTeachingLoads { get; set; }
        public virtual ICollection<MsCounselor> Counselor { get; set; }
        public virtual ICollection<TrGcReportStudent> GcReportStudentUserConsoller { get; set; }
        public virtual ICollection<TrGcReportStudent> GcReportStudentUserReport { get; set; }
        public virtual ICollection<MsUserPlatform> UserPlatforms { get; set; }
        public virtual ICollection<MsSanctionMappingAttentionBy> SanctionMappingAttentionBies { get; set; }
        public virtual ICollection<MsUserRole> UserRoles { get; set; }
        public virtual ICollection<TrCourseworkAnecdotalStudentComment> PortfolioComments { get; set; }
        public virtual ICollection<TrCourseworkAnecdotalStudentSeen> PortfolioSeens { get; set; }
        public virtual ICollection<TrReflectionStudentComment> ReflectionComments { get; set; }
        public virtual ICollection<MsStudentEmailAdditionalReceiver> StudentEmailAdditionalReceivers { get; set; }
        public virtual ICollection<TrExperience> TrExperiences { get; set; }
        public virtual ICollection<TrCasAdvisor> TrCasAdvisors { get; set; }
        public virtual ICollection<TrExperienceStatusChangeHs> TrExperienceStatusChangeHs { get; set; }
        public virtual ICollection<TrRequestDownloadExperience> TrRequestDownloadExperiences { get; set; }
        public virtual ICollection<TrEvidencesComment> TrEvidencesComments { get; set; }
        public virtual ICollection<TrEntryMeritStudent> EntryMeritStudents { get; set; }
        public virtual ICollection<TrServiceAsActionForm> ServiceAsActionForms { get; set; }
        public virtual ICollection<TrServiceAsActionForm> ServiceAsActionApproves { get; set; }
        public virtual ICollection<TrServiceAsActionComment> ServiceAsActionComments { get; set; }
    }

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

            builder.Property(x => x.Email)
                .HasMaxLength(128);

            base.Configure(builder);
        }
    }
}
