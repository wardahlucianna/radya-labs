using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class LtRole : CodeEntity, IUserEntity
    {
        public bool CanDeleted { get; set; }
        public string IdSchool { get; set; }
        public string IdRoleGroup { get; set; }
        public bool IsCanOpenTeacherTracking { get; set; }

        public virtual ICollection<MsUserRole> UserRoles { get; set; }
        public virtual ICollection<LtRoleSetting> RoleSettings { get; set; }
        public virtual ICollection<TrRolePosition> RolePositions { get; set; }
        public virtual ICollection<TrRolePermission> RolePermissions { get; set; }
        public virtual LtRoleGroup RoleGroup { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsMessageApprovalState> ApprovalStates { get; set; }
        public virtual ICollection<TrMessageApproval> MessageApprovals { get; set; }
        public virtual ICollection<TrRoleLoginAs> RoleLoginAsRole { get; set; }
        public virtual ICollection<TrRoleLoginAs> RoleLoginAsAuthorizedRole { get; set; }
    }

    internal class LtRoleConfiguration : CodeEntityConfiguration<LtRole>
    {
        public override void Configure(EntityTypeBuilder<LtRole> builder)
        {
            builder.Property(x => x.CanDeleted)
                .IsRequired();
            builder.HasOne(x => x.RoleGroup)
                .WithMany(x => x.Roles)
                .HasForeignKey(fk => fk.IdRoleGroup)
                .HasConstraintName("FK_LtRole_LtRoleGroup")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            builder.HasOne(x => x.School)
               .WithMany(x => x.Roles)
               .HasForeignKey(fk => fk.IdSchool)
               .HasConstraintName("FK_LtRole_MsSchool")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();
            base.Configure(builder);
        }
    }
}
