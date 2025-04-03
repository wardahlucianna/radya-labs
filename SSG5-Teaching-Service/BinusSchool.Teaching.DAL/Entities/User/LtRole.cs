using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities.User
{
    public class LtRole : CodeEntity, ITeachingEntity
    {
        public string IdSchool { get; set; }
        public string IdRoleGroup { get; set; }
        public virtual ICollection<MsUserRole> UserRoles { get; set; }
        public virtual LtRoleGroup RoleGroup { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsLessonPlanApproverSetting> MsLessonApproverSettings { get; set; }
        public virtual ICollection<TrRolePosition> RolePositions { get; set; }

    }

    internal class LtRoleConfiguration : CodeEntityConfiguration<LtRole>
    {
        public override void Configure(EntityTypeBuilder<LtRole> builder)
        {
            builder.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(128)
                .IsRequired();

            builder.HasOne(x => x.RoleGroup)
               .WithMany(x => x.Roles)
               .HasForeignKey(fk => fk.IdRoleGroup)
               .HasConstraintName("FK_LtRole_LtRoleGroup")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();
            builder.HasOne(x => x.School)
               .WithMany(x => x.Roles)
               .HasForeignKey(fk => fk.IdSchool)
               .HasConstraintName("FK_LtRole_MsSchool")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();
            base.Configure(builder);
        }
    }
}
