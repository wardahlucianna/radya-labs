using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities.Employee
{
    public class MsStaff : AuditNoUniqueEntity, ISchoolEntity
    {
        public string IdBinusian { get; set; }
        public string ShortName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BinusianEmailAddress { get; set; }
        public string PersonalEmailAddress { get; set; }
        public virtual ICollection<MsTextbookSettingApproval> TextbookSettingApprovals { get; set; }
        public virtual ICollection<MsTextbookUserPeriodDetail> TextbookUserPeriodDetails { get; set; }
        public virtual ICollection<TrTextbook> TextbooksUserApproval1 { get; set; }
        public virtual ICollection<TrTextbook> TextbooksUserApproval2 { get; set; }
        public virtual ICollection<TrTextbook> TextbooksUserApproval3 { get; set; }
        public virtual ICollection<TrTextbook> TextbooksUserCreate { get; set; }
        public virtual ICollection<MsVenueMappingApproval> VenueMappingApprovals { get; set; }

        public virtual ICollection<TrPublishSurveyMapping> PublishSurveyMappings { get; set; }
        public virtual ICollection<TrScheduleRealization2> ScheduleRealization2s { get; set; }
        public virtual ICollection<TrScheduleRealization2> ScheduleRealization2sSubtitutes { get; set; }
        public virtual ICollection<MsLessonTeacher> LessonTeachers { get; set; }
        public virtual ICollection<MsProjectFeedback> ProjectFeedbacks { get; set; }
        public virtual ICollection<MsSchoolProjectCoordinator> SchoolProjectCoordinators { get; set; }
    }

    internal class MsStaffConfiguration : AuditNoUniqueEntityConfiguration<MsStaff>
    {
        public override void Configure(EntityTypeBuilder<MsStaff> builder)
        {

            builder.HasKey(x => x.IdBinusian);

            builder.Property(x => x.IdBinusian)
                .HasMaxLength(36);

            builder.Property(x => x.ShortName)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.LastName)
               .HasMaxLength(100);

            builder.Property(x => x.BinusianEmailAddress)
             .HasMaxLength(60);

            builder.Property(x => x.PersonalEmailAddress)
               .HasMaxLength(60);

            base.Configure(builder);
        }
    }
}
