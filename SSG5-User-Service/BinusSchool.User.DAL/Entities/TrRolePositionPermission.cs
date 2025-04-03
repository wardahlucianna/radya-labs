using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class TrRolePositionPermission : AuditEntity, IUserEntity
    {
        public string IdFeaturePermission { get; set; }
        public string IdRolePosition { get; set; }
        public string Type { get; set; }

        public virtual MsFeaturePermission FeaturePermission { get; set; }
        public virtual TrRolePosition RolePosition { get; set; }
    }

    internal class TrRolePositionPermissionConfiguration : AuditEntityConfiguration<TrRolePositionPermission>
    {
        public override void Configure(EntityTypeBuilder<TrRolePositionPermission> builder)
        {
            builder.HasOne(x => x.RolePosition)
             .WithMany(x => x.RolePositionPermissions)
             .HasForeignKey(fk => fk.IdRolePosition)
             .HasConstraintName("FK_TrRolePositionPermission_TrRolePosition")
             .OnDelete(DeleteBehavior.NoAction)
             .IsRequired();

            builder.HasOne(x => x.FeaturePermission)
             .WithMany(x => x.RolePositionPermissions)
             .HasForeignKey(fk => fk.IdFeaturePermission)
             .HasConstraintName("FK_TrRolePositionPermission_MsFeaturePermission")
             .OnDelete(DeleteBehavior.NoAction)
             .IsRequired();

            builder.Property(x => x.Type)
              .HasMaxLength(40);

            base.Configure(builder);
        }
    }
}
