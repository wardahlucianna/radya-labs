using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Scheduling;
using BinusSchool.Persistence.SchoolDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities.Teaching
{
    public class MsTeacherPosition : CodeEntity, ISchoolEntity
    {
        public string IdPosition { get; set; }
        public string IdSchool { get; set; }
        public virtual LtPosition Position { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsMeritDemeritApprovalSetting> Approval1 { get; set; }
        public virtual ICollection<MsMeritDemeritApprovalSetting> Approval2 { get; set; }
        public virtual ICollection<MsMeritDemeritApprovalSetting> Approval3 { get; set; }
        public virtual ICollection<MsSanctionMappingAttentionBy> SanctionMappingAttentionBies { get; set; }
        public virtual ICollection<MsTextbookUserPeriodDetail> TextbookUserPeriodDetails { get; set; }
        public virtual ICollection<MsTextbookSettingApproval> TextbookSettingApprovals { get; set; }
        public virtual ICollection<TrPublishSurveyPosition> PublishSurveyPositions { get; set; }
        public virtual ICollection<TrPublishSurveyUser> PublishSurveyUsers { get; set; }
        public virtual ICollection<TrRolePosition> RolePositions { get; set; }
    }

    internal class MsTeacherPositionConfiguration : CodeEntityConfiguration<MsTeacherPosition>
    {
        public override void Configure(EntityTypeBuilder<MsTeacherPosition> builder)
        {
           
            builder.HasOne(x => x.Position)
                .WithMany(x => x.TeacherPositions)
                .HasForeignKey(fk => fk.IdPosition)
                .HasConstraintName("FK_MsTeacherPosition_LtPosition")
                .OnDelete(DeleteBehavior.Cascade)
                 .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.TeacherPositions)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsTeacherPosition_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                 .IsRequired();

            base.Configure(builder);
        }
    }
}
