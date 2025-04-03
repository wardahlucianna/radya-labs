using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.Scheduling;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Persistence.TeachingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities
{
    public class MsTeacherPosition : CodeEntity, ITeachingEntity
    {
        public string IdPosition { get; set; }
        public string IdSchool { get; set; }
        public virtual LtPosition Position { get; set; }
        public virtual ICollection<MsNonTeachingLoad> NonTeachingLoads { get; set; }
        public virtual MsSchool School { get; set; }
        public AcademicType Category { get; set; }
        public virtual ICollection<MsTeacherPositionAlias> TeacherPositionAliases { get; set; }
        public virtual ICollection<MsHomeroomTeacher> HomeroomTeachers { get; set; }
        public virtual ICollection<MsLessonPlanApproverSetting> MsLessonApproverSettings { get; set; }
        public virtual ICollection<TrRolePosition> RolePositions { get; set; }

    }

    internal class MsTeacherPositionConfiguration : CodeEntityConfiguration<MsTeacherPosition>
    {
        public override void Configure(EntityTypeBuilder<MsTeacherPosition> builder)
        {
            builder.HasOne(x => x.School)
                 .WithMany(x => x.TeacherPositions)
                 .HasForeignKey(fk => fk.IdSchool)
                 .HasConstraintName("FK_MsTeacherPosition_MsSchool")
                 .OnDelete(DeleteBehavior.NoAction)
                  .IsRequired();

            builder.HasOne(x => x.Position)
                .WithMany(x => x.TeacherPositions)
                .HasForeignKey(fk => fk.IdPosition)
                .HasConstraintName("FK_MsTeacherPosition_LtPosition")
                .OnDelete(DeleteBehavior.Cascade)
                 .IsRequired();

            base.Configure(builder);
        }
    }
}
