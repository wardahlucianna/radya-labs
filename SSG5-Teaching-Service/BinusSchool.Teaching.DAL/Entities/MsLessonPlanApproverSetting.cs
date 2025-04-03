using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.Employee;
using BinusSchool.Persistence.TeachingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities
{
    public class MsLessonPlanApproverSetting : AuditEntity, ITeachingEntity
    {
        public string IdRole { get; set; }
        public string IdTeacherPosition { get; set; }
        public string IdBinusian { get; set; }

        public virtual LtRole Role { get; set; }
        public virtual MsTeacherPosition TeacherPosition { get; set; }
        public virtual MsStaff Staff { get; set; }
    }

    internal class MsLessonPlanApproverSettingConfiguration : AuditEntityConfiguration<MsLessonPlanApproverSetting>
    {
        public override void Configure(EntityTypeBuilder<MsLessonPlanApproverSetting> builder)
        {
            builder.HasOne(x => x.Role)
                .WithMany(x => x.MsLessonApproverSettings)
                .HasForeignKey(x => x.IdRole)
                .HasConstraintName("FK_MsLessonPlanApproverSetting_LtRole")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.TeacherPosition)
                .WithMany(x => x.MsLessonApproverSettings)
                .HasForeignKey(x => x.IdTeacherPosition)
                .HasConstraintName("FK_MsLessonPlanApproverSetting_MsTeacherPosition")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Staff)
                .WithMany(x => x.MsLessonApproverSettings)
                .HasForeignKey(x => x.IdBinusian)
                .HasConstraintName("FK_MsLessonPlanApproverSetting_MsStaff")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
