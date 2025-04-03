using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.Employee;
using BinusSchool.Persistence.UserDb.Entities.Student;
using BinusSchool.Persistence.UserDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities.School
{
    public class MsSchool : AuditEntity, IUserEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Logo { get; set; }
        public virtual ICollection<MsAcademicYear> AcademicYears { get; set; }
        public virtual ICollection<MsTeacherPosition> TeacherPositions { get; set; }
        public virtual ICollection<LtRole> Roles { get; set; }
        public virtual ICollection<MsFeedbackType> FeedbackTypes { get; set; }
        public virtual ICollection<MsMessageCategory> MessageCategories { get; set; }
        public virtual ICollection<MsMessageApproval> MessageApprovals { get; set; }
        public virtual ICollection<MsClassroom> Classrooms { get; set; }
        public virtual ICollection<MsUserSchool> UserSchools { get; set; }
        public virtual ICollection<MsMessageOption> MessageOptions { get; set; }
        public virtual ICollection<MsStudent> Students { get; set; }
        public virtual ICollection<MsStaff> Staffs { get; set; }
        public virtual ICollection<MsFeatureSchool> FeatureSchools { get; set; }
        public virtual ICollection<TrNotification> Notifications { get; set; }
        public virtual ICollection<MsBlockingCategory> BlockingCategorys { get; set; }
        public virtual ICollection<MsBlockingMessage> BlockingMessages { get; set; }
        public virtual ICollection<MsBlockingType> BlockingTypes { get; set; }
        public virtual ICollection<MsGroupMailingList> GroupMailingLists { get; set; }
        public virtual ICollection<TrLogQueueMessage> LogQueueMessages { get; set; }
        public virtual ICollection<TrLogQueueMsgNotif> LogQueueMsgNotif { get; set; }

    }

    internal class MsSchoolConfiguration : AuditEntityConfiguration<MsSchool>
    {
        public override void Configure(EntityTypeBuilder<MsSchool> builder)
        {
            builder.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Logo)
                .HasMaxLength(900);

            builder.Property(x => x.Description)
                .HasMaxLength(128)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
