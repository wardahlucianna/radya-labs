using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class MsFeaturePermission : AuditEntity, IUserEntity
    {
        public string IdFeature { get; set; }
        public string IdPermission { get; set; }

        public virtual MsFeature Feature { get; set; }
        public virtual LtPermission Permission { get; set; }
        public virtual ICollection<TrRolePositionPermission> RolePositionPermissions { get; set; }
        public virtual ICollection<TrRolePermission> RolePermissions { get; set; }
    }

    internal class MsFeaturePermissionConfiguration : AuditEntityConfiguration<MsFeaturePermission>
    {
        public override void Configure(EntityTypeBuilder<MsFeaturePermission> builder)
        {
            builder.HasOne(x => x.Feature)
                .WithMany(x => x.FeaturePermissions)
                .HasForeignKey(fk => fk.IdFeature)
                .HasConstraintName("FK_MsFeaturePermission_MsFeature")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Permission)
                .WithMany(x => x.FeaturePermissions)
                .HasForeignKey(fk => fk.IdPermission)
                .HasConstraintName("FK_MsFeaturePermission_MsPermission")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
